using System.Text.Json;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;

// =========================================================================================
// SECTION 1: SYSTEM INITIALIZATION & BOOTSTRAPPER
// =========================================================================================

Console.Title = "Reality Engine v1.4 - Stability Patch";
Console.CursorVisible = false;

InitializeEnvironment();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
  _____  ______          _      _____ _______        ______ _   _  _____ _____ _   _ ______ 
 |  __ \|  ____|   /\   | |    |_   _|__   __\ \  / /  ____| \ | |/ ____|_   _| \ | |  ____|
 | |__) | |__     /  \  | |      | |    | |   \ \/ /| |__  |  \| | |  __  | | |  \| | |__   
 |  _  /|  __|   / /\ \ | |      | |    | |    >  < |  __| | . ` | | |_ | | | | . ` |  __|  
 | | \ \| |____ / ____ \| |____ _| |_   | |   / /\ \| |____| |\  | |__| |_| |_| |\  | |____ 
 |_|  \_\______/_/    \_\______|_____|  |_|  /_/  \_\______|_| \_|\_____|_____|_| \_|______|
");
Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine(" --------------------------------------------------------------------------");
Console.WriteLine("                       [ UNIVERSAL CHARACTER ENGINE ]                      ");
Console.WriteLine("                                Version 1.4                                ");
Console.WriteLine(" --------------------------------------------------------------------------");
Console.WriteLine("\n\n        >> SYSTEM INITIALIZED. PRESS ANY KEY TO ENTER THE GRID <<        ");
Console.ResetColor();
Console.ReadKey(true);

// =========================================================================================
// SECTION 2: GLOBAL STATE & MAIN LOOP
// =========================================================================================

UniverseData activeModule = null;
bool keepRunning = true;
bool isDebugMode = false;
Random rand = new Random();

while (keepRunning)
{
    string status = activeModule == null ? "None (Please load a module)" : activeModule.UniverseName;
    string debugStatus = isDebugMode ? "[ON]" : "[OFF]";
    string header = $"--- REALITY ENGINE v1.4 ---\nACTIVE MODULE: {status}";

    string[] menuOptions = {
        "Generate Entity",
        "Load Universe Module",
        "Architect Mode (Build New Universe)",
        $"Toggle Debug Mode {debugStatus}",
        "Help Library",
        "Exit"
    };

    int choice = DrawInteractiveMenu(header, menuOptions);

    switch (choice)
    {
        case 0:
            if (activeModule == null)
            {
                ClearScreen();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[!] Error: No module loaded! Diverting to Load Menu...");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1200);
                LoadModuleFlow();
            }
            else { GenerateEntity(activeModule, rand); }
            break;
        case 1: LoadModuleFlow(); break;
        case 2: ArchitectModeFlow(); break;
        case 3: isDebugMode = !isDebugMode; break;
        case 4: ShowInstructions(); break;
        case 5: keepRunning = false; break;
    }
}

Console.CursorVisible = true;
Environment.Exit(0);


// =========================================================================================
// SECTION 3: THE UI FRAMEWORK & CORE HELPERS
// =========================================================================================

// THE MASTER FIX: This function completely wipes the history buffer to stop ghosting.
static void ClearScreen()
{
    try
    {
        if (OperatingSystem.IsWindows())
        {
            Console.BufferHeight = Console.WindowHeight;
        }
    }
    catch { /* Ignore if the terminal doesn't support buffer resizing */ }

    Console.Clear();
    Console.SetCursorPosition(0, 0);
}

static void InitializeEnvironment()
{
    string[] requiredFolders = { "UniverseFiles", "Docs", "Saves" };
    foreach (var folder in requiredFolders)
    {
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
    }

    string defaultHelpPath = Path.Combine("Docs", "00_Start_Here.txt");
    if (Directory.GetFiles("Docs", "*.txt").Length == 0)
    {
        File.WriteAllText(defaultHelpPath, "Welcome to Reality Engine v1.4.\n\nUse the arrow keys to navigate the menus. To add more worlds, place your .json files in the 'UniverseFiles' folder or use Architect Mode to build one.");
    }
}

static int DrawInteractiveMenu(string title, string[] options)
{
    int selectedIndex = 0;
    ConsoleKey key;

    do
    {
        ClearScreen();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(title);
        Console.WriteLine(new string('=', Math.Max(27, title.Split('\n').Max(s => s.Length))));
        Console.WriteLine();
        Console.ResetColor();

        for (int i = 0; i < options.Length; i++)
        {
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Cyan;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($" > {options[i]} ");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"   {options[i]} ");
            }
        }

        key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.UpArrow) selectedIndex = Math.Max(0, selectedIndex - 1);
        else if (key == ConsoleKey.DownArrow) selectedIndex = Math.Min(options.Length - 1, selectedIndex + 1);

    } while (key != ConsoleKey.Enter);

    return selectedIndex;
}

static string PromptUser(string message)
{
    Console.CursorVisible = true;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"\n{message} ");
    Console.ResetColor();
    string input = Console.ReadLine();
    Console.CursorVisible = false;
    return input;
}


// =========================================================================================
// SECTION 4: THE ARCHITECT WIZARD 
// =========================================================================================

void ArchitectModeFlow()
{
    ClearScreen();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==================================================");
    Console.WriteLine("          ARCHITECT MODE: UNIVERSE BUILDER        ");
    Console.WriteLine("==================================================");
    Console.ResetColor();
    Console.WriteLine("This wizard will generate a perfectly formatted JSON foundation.");

    string uniName = PromptUser("Enter Universe Name (e.g., Cyberpunk 2099):");
    if (string.IsNullOrWhiteSpace(uniName)) return;

    string archTitle = PromptUser("What are the character classes called? (e.g., Job, Species):");
    string archName = PromptUser($"Enter the name of your first {archTitle} (e.g., Hacker, Elf):");

    string firstNamesRaw = PromptUser("Enter 3-5 First Names (separated by commas):");
    var firstNames = firstNamesRaw.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n));

    string lastNamesRaw = PromptUser("Enter 3-5 Last Names (separated by commas):");
    var lastNames = lastNamesRaw.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n));

    string newJson = $$"""
{
  "UniverseName": "{{uniName}}",
  "ArchetypeTitle": "{{archTitle}}",
  "Modules": {
    "ShowRarity": true,
    "UseNarrative": true,
    "UsePointPools": true
  },
  "Archetypes": {
    "{{archName}}": {
      "NicknameChance": 0.5,
      "GenderOdds": { "Male": 45, "Female": 45, "Non-Binary": 10 },
      "FirstNames": [ {{string.Join(", ", firstNames.Select(n => $"\"{n}\""))}} ],
      "LastNames": [ {{string.Join(", ", lastNames.Select(n => $"\"{n}\""))}} ],
      "StartingTags": [ "Base-Tag" ],
      "MandatoryTraits": {
        "Primary Weapon": [ 
          { "Value": "Basic Tool", "Weight": 50, "Rarity": "Common" },
          { "Value": "Epic Tool", "Weight": 5, "GrantsTag": "Elite", "Rarity": "Epic" }
        ]
      },
      "PointPools": {
        "Core Stats": {
          "TotalPoints": 30,
          "MinPerAttribute": 5,
          "MaxPerAttribute": 15,
          "Attributes": [ "Strength", "Agility", "Intelligence" ]
        }
      }
    }
  },
  "BioTemplates": {
    "1_Intro": [
      { "Text": "{Full} was always known for carrying a {Primary Weapon}.", "Weight": 10 },
      { "Text": "As an {Elite}, {Nick} handles their {Primary Weapon} with unmatched skill.", "Requires": "Elite", "Weight": 50 }
    ]
  }
}
""";

    string fileName = uniName.Replace(" ", "_").Replace(":", "") + ".json";
    string filePath = Path.Combine("UniverseFiles", fileName);
    File.WriteAllText(filePath, newJson);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[SUCCESS] Foundation built and saved to: {filePath}");
    Console.ResetColor();
    Console.WriteLine("You can now load this module, or edit the JSON file to add more complexity.");
    Console.WriteLine("Press any key to return to the Main Menu...");
    Console.ReadKey(true);
}


// =========================================================================================
// SECTION 5: MODULE LOADING & THE PRE-FLIGHT VALIDATOR
// =========================================================================================

void LoadModuleFlow()
{
    string filePath = SelectUniverse();
    if (filePath == null) return;

    try
    {
        string jsonString = File.ReadAllText(filePath);
        activeModule = JsonSerializer.Deserialize<UniverseData>(jsonString);

        ClearScreen();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"SUCCESSFULLY LOADED: {activeModule.UniverseName}");
        Console.WriteLine("--------------------------------------------------");
        Console.ResetColor();

        RunValidator(activeModule);

        long totalUniverseCombinations = 0;
        Console.WriteLine($"Archetypes Found: {activeModule.Archetypes.Count}\n");
        foreach (var arch in activeModule.Archetypes)
        {
            long archCombos = 1;
            int firsts = arch.Value.FirstNames?.Count ?? 1;
            int lasts = arch.Value.LastNames?.Count ?? 1;
            int nicks = arch.Value.Nicknames?.Count ?? 0;

            archCombos *= firsts * lasts * (nicks > 0 ? nicks : 1);

            if (arch.Value.MandatoryTraits != null)
                foreach (var trait in arch.Value.MandatoryTraits)
                    archCombos *= (trait.Value.Count > 0 ? trait.Value.Count : 1);

            totalUniverseCombinations += archCombos;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"> {arch.Key}:");
            Console.ResetColor();
            Console.WriteLine($"  - Total Archetype Combinations: {archCombos:N0}");
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[ TOTAL UNIVERSE COMBINATIONS: {totalUniverseCombinations:N0}+ ]");
        Console.ResetColor();

        Console.WriteLine("\nSystem Ready. Press any key to return to menu...");
        Console.ReadKey(true);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nCRITICAL ERROR LOADING JSON: {ex.Message}");
        Console.ResetColor();
        Console.ReadKey(true);
    }
}

void RunValidator(UniverseData data)
{
    Console.WriteLine("Running Pre-Flight Diagnostics...");
    List<string> warnings = new List<string>();

    foreach (var archEntry in data.Archetypes)
    {
        string aName = archEntry.Key;
        var arch = archEntry.Value;

        List<string> grantedTags = new List<string>();
        List<string> requiredTags = new List<string>();

        if (arch.StartingTags != null) grantedTags.AddRange(arch.StartingTags);
        if (arch.GenderOdds != null) grantedTags.AddRange(arch.GenderOdds.Keys);

        if (arch.MandatoryTraits != null)
        {
            foreach (var traitList in arch.MandatoryTraits.Values)
            {
                foreach (var item in traitList.Select(e => ResolveElement(e)))
                {
                    if (!string.IsNullOrEmpty(item.GrantsTag)) grantedTags.Add(item.GrantsTag);
                    if (!string.IsNullOrEmpty(item.Requires)) requiredTags.Add(item.Requires);
                }
            }
        }

        if (arch.OptionalTraits != null)
        {
            foreach (var optTrait in arch.OptionalTraits.Values)
            {
                foreach (var item in optTrait.Options.Select(e => ResolveElement(e)))
                {
                    if (!string.IsNullOrEmpty(item.GrantsTag)) grantedTags.Add(item.GrantsTag);
                    if (!string.IsNullOrEmpty(item.Requires)) requiredTags.Add(item.Requires);
                }
            }
        }

        foreach (var req in requiredTags.Distinct())
        {
            if (!grantedTags.Contains(req))
            {
                warnings.Add($"[{aName}] Dead Item: An item requires '{req}', but nothing in the archetype grants this tag.");
            }
        }

        if (data.Modules.UsePointPools && arch.PointPools != null)
        {
            foreach (var pool in arch.PointPools)
            {
                int maxPossible = pool.Value.MaxPerAttribute * pool.Value.Attributes.Count;
                if (pool.Value.TotalPoints >= maxPossible)
                {
                    warnings.Add($"[{aName}] Math Error: '{pool.Key}' Budget ({pool.Value.TotalPoints}) is >= Max Capacity ({maxPossible}).");
                }
            }
        }
    }

    if (warnings.Count > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n⚠️ WARNINGS DETECTED:");
        foreach (var w in warnings) Console.WriteLine($" - {w}");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✔️ Diagnostics Passed: No logic loops detected.");
        Console.ResetColor();
    }
    Console.WriteLine("--------------------------------------------------");
}

string SelectUniverse()
{
    string folder = "UniverseFiles";
    string[] files = Directory.GetFiles(folder, "*.json");
    if (files.Length == 0) return null;

    List<string> options = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
    options.Add("[ Back to Main Menu ]");

    int choice = DrawInteractiveMenu("--- SELECT A UNIVERSE MODULE ---", options.ToArray());
    if (choice == options.Count - 1) return null;
    return files[choice];
}

void ShowInstructions()
{
    string folder = "Docs";
    while (true)
    {
        string[] files = Directory.GetFiles(folder, "*.txt").OrderBy(f => f).ToArray();
        List<string> options = files.Select(f => Path.GetFileNameWithoutExtension(f).Replace("_", " ")).ToList();
        options.Add("[ Back to Main Menu ]");

        int choice = DrawInteractiveMenu("--- REALITY ENGINE HELP LIBRARY ---", options.ToArray());
        if (choice == options.Count - 1) break;

        ClearScreen();
        Console.ForegroundColor = ConsoleColor.Gray;

        string content = File.ReadAllText(files[choice]);
        Console.WriteLine(content);

        Console.ResetColor();
        Console.WriteLine("\n--------------------------------------------------");
        Console.WriteLine(">> Press any key to return to the Help Library...");
        Console.ReadKey(true);
    }
}


// =========================================================================================
// SECTION 6: THE GENERATOR ENGINE 
// =========================================================================================

void LogDebug(string message)
{
    if (!isDebugMode) return;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("[DEBUG] ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(message);
    Console.ResetColor();
    System.Threading.Thread.Sleep(100);
}

void GenerateEntity(UniverseData data, Random rand)
{
    GeneratedEntity entity = new GeneratedEntity();
    List<string> activeTags = new List<string>();

    string[] typeOptions = data.Archetypes.Keys.ToArray();
    int choice = DrawInteractiveMenu($"--- {data.UniverseName} ---\nSelect an {data.ArchetypeTitle}:", typeOptions);
    string selectedTypeName = typeOptions[choice];

    ClearScreen();
    if (isDebugMode) Console.WriteLine($"=== INITIATING GENERATION SEQUENCE: {selectedTypeName.ToUpper()} ===\n");

    EntityArchetype archetype = data.Archetypes[selectedTypeName];

    if (archetype.StartingTags != null)
    {
        activeTags.AddRange(archetype.StartingTags);
        LogDebug($"Applied Starting Tags: {string.Join(", ", archetype.StartingTags)}");
    }
    entity.Attributes.Add(data.ArchetypeTitle, selectedTypeName);

    if (archetype.GenderOdds != null && archetype.GenderOdds.Count > 0)
    {
        int totalGenderWeight = archetype.GenderOdds.Values.Sum();
        int genderRoll = rand.Next(0, totalGenderWeight);
        int cursor = 0;
        foreach (var entry in archetype.GenderOdds)
        {
            cursor += entry.Value;
            if (genderRoll < cursor) { entity.Gender = entry.Key; break; }
        }
    }
    else { entity.Gender = "Neutral"; }

    activeTags.Add(entity.Gender);
    AssignPronouns(entity);
    LogDebug($"Rolled Gender: {entity.Gender} (Added to Tags)");

    entity.FirstName = PickSmart("First Name", archetype.FirstNames, activeTags, rand, data.Modules.ShowRarity).Value;
    entity.LastName = PickSmart("Last Name", archetype.LastNames, activeTags, rand, data.Modules.ShowRarity).Value;

    if (archetype.Nicknames != null && rand.NextDouble() < archetype.NicknameChance)
        entity.Nickname = PickSmart("Nickname", archetype.Nicknames, activeTags, rand, data.Modules.ShowRarity).Value;
    else
    {
        entity.Nickname = "";
        LogDebug($"Nickname: Skipped (Failed probability roll)");
    }

    if (archetype.MandatoryTraits != null)
    {
        foreach (var trait in archetype.MandatoryTraits)
        {
            SmartOption picked = PickSmart(trait.Key, trait.Value, activeTags, rand, data.Modules.ShowRarity);
            string displayValue = data.Modules.ShowRarity && !string.IsNullOrEmpty(picked.Rarity) ? $"{picked.Value} [{picked.Rarity}]" : picked.Value;
            entity.Attributes.Add(trait.Key, displayValue);
        }
    }

    if (archetype.OptionalTraits != null)
    {
        foreach (var trait in archetype.OptionalTraits)
        {
            if (rand.NextDouble() < trait.Value.Chance)
            {
                SmartOption picked = PickSmart(trait.Key, trait.Value.Options, activeTags, rand, data.Modules.ShowRarity);
                string displayValue = data.Modules.ShowRarity && !string.IsNullOrEmpty(picked.Rarity) ? $"{picked.Value} [{picked.Rarity}]" : picked.Value;
                entity.Attributes.Add(trait.Key, displayValue);
            }
            else LogDebug($"Optional Trait '{trait.Key}': Skipped (Failed probability roll)");
        }
    }

    if (data.Modules.UsePointPools && archetype.PointPools != null)
    {
        LogDebug("Calculating RPG Point Pools...");
        foreach (var poolEntry in archetype.PointPools)
        {
            PointPool pool = poolEntry.Value;
            Dictionary<string, int> scores = pool.Attributes.ToDictionary(a => a, a => pool.MinPerAttribute);
            int remaining = pool.TotalPoints - (pool.Attributes.Count * pool.MinPerAttribute);

            while (remaining > 0)
            {
                string target = pool.Attributes[rand.Next(pool.Attributes.Count)];
                if (scores[target] < pool.MaxPerAttribute) { scores[target]++; remaining--; }
                if (scores.Values.All(v => v == pool.MaxPerAttribute)) break;
            }
            foreach (var s in scores) entity.Attributes.Add($"{poolEntry.Key}: {s.Key}", s.Value.ToString());
            LogDebug($"Pool [{poolEntry.Key}] Distributed {pool.TotalPoints} points across {pool.Attributes.Count} attributes.");
        }
    }

    entity.Tags = activeTags.Distinct().ToList();

    if (data.Modules.UseNarrative)
    {
        LogDebug("Stitching Narrative using Stat-Gating...");
        entity.Story = ConstructStory(entity, data, rand);
        LogDebug("Narrative constructed successfully.");
    }

    if (isDebugMode)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[ GENERATION COMPLETE ]");
        Console.ResetColor();
        Console.WriteLine("Press any key to view the formatted Character Sheet...");
        Console.ReadKey(true);
    }

    DisplayEntity(entity, data.UniverseName);
}


// =========================================================================================
// SECTION 7: LOGIC GATES & RESOLVERS 
// =========================================================================================

string ConstructStory(GeneratedEntity entity, UniverseData data, Random rand)
{
    if (data.BioTemplates == null) return null;
    List<string> storyBits = new List<string>();

    foreach (var category in data.BioTemplates)
    {
        var valid = category.Value.Where(t =>
        {
            bool tagMet = string.IsNullOrEmpty(t.Requires) || entity.Tags.Contains(t.Requires);
            bool statMet = true;
            if (!string.IsNullOrEmpty(t.MinStatName))
            {
                // DEEP FIX: Ensures we perfectly match the exact stat name, avoiding partial matches.
                var entry = entity.Attributes.FirstOrDefault(a => a.Key.EndsWith($": {t.MinStatName}") || a.Key == t.MinStatName);
                statMet = entry.Key != null && int.TryParse(entry.Value, out int score) && score >= t.MinStatValue;
            }
            return tagMet && statMet;
        }).ToList();

        if (valid.Count == 0) continue;

        int total = valid.Sum(t => t.Weight);
        int roll = rand.Next(0, total);
        int cursor = 0;
        string pickedText = valid[0].Text;
        foreach (var t in valid) { cursor += t.Weight; if (roll < cursor) { pickedText = t.Text; break; } }

        pickedText = pickedText.Replace("{First}", entity.FirstName)
                               .Replace("{Last}", entity.LastName)
                               .Replace("{Nick}", entity.NicknameOrFirst)
                               .Replace("{Nickname}", entity.NicknameOrFirst)
                               .Replace("{Full}", entity.FullName)
                               .Replace("{Gender}", entity.Gender);

        foreach (var p in entity.Pronouns) pickedText = pickedText.Replace($"{{{p.Key}}}", p.Value);
        foreach (var a in entity.Attributes) pickedText = pickedText.Replace($"{{{a.Key}}}", a.Value);

        storyBits.Add(pickedText);
    }
    return string.Join(" ", storyBits);
}

void AssignPronouns(GeneratedEntity entity)
{
    string gen = entity.Gender.ToLower();

    if (gen == "male" || gen == "man" || gen == "boy")
    {
        entity.Pronouns = new Dictionary<string, string> { { "Subject", "he" }, { "Object", "him" }, { "Possessive", "his" } };
    }
    else if (gen == "female" || gen == "woman" || gen == "girl")
    {
        entity.Pronouns = new Dictionary<string, string> { { "Subject", "she" }, { "Object", "her" }, { "Possessive", "her" } };
    }
    else if (gen.Contains("machine") || gen.Contains("ai") || gen.Contains("inert") ||
             gen.Contains("ship") || gen.Contains("vehicle") || gen.Contains("robot") ||
             gen.Contains("weapon") || gen.Contains("artifact"))
    {
        entity.Pronouns = new Dictionary<string, string> { { "Subject", "it" }, { "Object", "it" }, { "Possessive", "its" } };
    }
    else
    {
        entity.Pronouns = new Dictionary<string, string> { { "Subject", "they" }, { "Object", "them" }, { "Possessive", "their" } };
    }
}

SmartOption PickSmart(string categoryName, List<JsonElement> elements, List<string> activeTags, Random rand, bool showRarity)
{
    if (elements == null || elements.Count == 0) return new SmartOption { Value = "N/A" };

    var options = elements.Select(e => ResolveElement(e)).ToList();

    if (isDebugMode)
    {
        var blocked = options.Where(o => !string.IsNullOrEmpty(o.Requires) && !activeTags.Contains(o.Requires)).Select(o => o.Value).ToList();
        if (blocked.Count > 0) LogDebug($"Blocked {categoryName} options due to missing tags: {string.Join(", ", blocked)}");
    }

    var validOptions = options.Where(o => string.IsNullOrEmpty(o.Requires) || activeTags.Contains(o.Requires)).ToList();
    if (validOptions.Count == 0) return new SmartOption { Value = "None" };

    int totalWeight = validOptions.Sum(o => o.Weight);
    int roll = rand.Next(0, totalWeight);
    int cursor = 0;

    foreach (var opt in validOptions)
    {
        cursor += opt.Weight;
        if (roll < cursor)
        {
            if (!string.IsNullOrEmpty(opt.GrantsTag) && !activeTags.Contains(opt.GrantsTag))
            {
                activeTags.Add(opt.GrantsTag);
                LogDebug($"Rolled {categoryName}: {opt.Value} (Granted Tag: {opt.GrantsTag})");
            }
            else LogDebug($"Rolled {categoryName}: {opt.Value}");

            return opt;
        }
    }
    return validOptions[0];
}

SmartOption ResolveElement(JsonElement element)
{
    if (element.ValueKind == JsonValueKind.String) return new SmartOption { Value = element.GetString(), Weight = 1 };

    return new SmartOption
    {
        Value = element.TryGetProperty("Value", out var v) ? v.GetString() : "Unknown",
        Weight = element.TryGetProperty("Weight", out var w) ? w.GetInt32() : 1,
        Requires = element.TryGetProperty("Requires", out var r) ? r.GetString() : null,
        GrantsTag = element.TryGetProperty("GrantsTag", out var g) ? g.GetString() : null,
        Rarity = element.TryGetProperty("Rarity", out var rar) ? rar.GetString() : null
    };
}


// =========================================================================================
// SECTION 8: FINAL OUTPUT & SAVING 
// =========================================================================================

void DisplayEntity(GeneratedEntity entity, string universeName)
{
    ClearScreen(); // Ensure buffer is wiped before displaying the clean sheet

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                    [ IDENTIFICATION RECORD ]                     ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
    Console.ResetColor();

    Console.WriteLine($"   NAME   : {entity.FullName}");
    Console.WriteLine($"   GENDER : {entity.Gender}");
    if (entity.Tags.Count > 0)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"   TAGS   : [{string.Join("] [", entity.Tags)}]");
        Console.ResetColor();
    }
    Console.WriteLine();

    if (!string.IsNullOrEmpty(entity.Story))
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("┌─── [ NARRATIVE LOG ] ────────────────────────────────────────────┐");
        Console.ResetColor();

        int consoleWidth = 64;
        // DEEP FIX: Split safely by spaces and line breaks so word-wrap doesn't break
        string[] words = entity.Story.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        string currentLine = "   ";
        foreach (var word in words)
        {
            if (currentLine.Length + word.Length > consoleWidth)
            {
                Console.WriteLine(currentLine);
                currentLine = "   " + word + " ";
            }
            else { currentLine += word + " "; }
        }
        Console.WriteLine(currentLine);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("└──────────────────────────────────────────────────────────────────┘\n");
        Console.ResetColor();
    }

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("┌─── [ ASSETS & ATTRIBUTES ] ──────────────────────────────────────┐");
    Console.ResetColor();

    foreach (var attr in entity.Attributes.Where(a => !a.Key.Contains(":")))
    {
        Console.WriteLine($"   {attr.Key.PadRight(20)}: {attr.Value}");
    }

    var poolGroups = entity.Attributes.Where(a => a.Key.Contains(":")).GroupBy(a => a.Key.Split(':')[0].Trim());
    foreach (var group in poolGroups)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   --- {group.Key.ToUpper()} ---");
        Console.ResetColor();
        foreach (var item in group)
        {
            Console.WriteLine($"   {item.Key.Split(':')[1].Trim().PadRight(20)}: {item.Value}");
        }
    }

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("└──────────────────────────────────────────────────────────────────┘\n");
    Console.ResetColor();

    Console.WriteLine("Press 'S' to Save to file or any other key to return to Main Menu...");
    if (Console.ReadKey(true).Key == ConsoleKey.S) SaveEntityToFile(entity, universeName);
}

void SaveEntityToFile(GeneratedEntity entity, string universeName)
{
    string baseFolder = "Saves";
    string universeFolder = Path.Combine(baseFolder, universeName.Replace(":", ""));
    if (!Directory.Exists(universeFolder)) Directory.CreateDirectory(universeFolder);

    string fileName = $"{entity.FullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
    string fullPath = Path.Combine(universeFolder, fileName);

    using (StreamWriter writer = new StreamWriter(fullPath))
    {
        writer.WriteLine("==================================================================");
        writer.WriteLine($"   RECORD: {entity.FullName.ToUpper()}");
        writer.WriteLine($"   GENDER: {entity.Gender}");
        writer.WriteLine($"   TAGS: [{string.Join("] [", entity.Tags)}]");
        writer.WriteLine("==================================================================\n");

        if (!string.IsNullOrEmpty(entity.Story))
        {
            writer.WriteLine("[ NARRATIVE ]");
            writer.WriteLine(entity.Story);
            writer.WriteLine("\n------------------------------------------------------------------\n");
        }

        writer.WriteLine("[ ATTRIBUTES ]");
        foreach (var a in entity.Attributes) writer.WriteLine($"{a.Key.PadRight(20)}: {a.Value}");
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[SUCCESS] Character archived to: {fullPath}");
    Console.ResetColor();
    System.Threading.Thread.Sleep(1500);
}


// =========================================================================================
// SECTION 9: DATA MODELS
// =========================================================================================

public class GeneratedEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Nickname { get; set; }
    public string Gender { get; set; }
    public string Story { get; set; }
    public Dictionary<string, string> Pronouns { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    public List<string> Tags { get; set; } = new List<string>();
    public string FullName => string.IsNullOrEmpty(Nickname) ? $"{FirstName} {LastName}" : $"{FirstName} '{Nickname}' {LastName}";
    public string NicknameOrFirst => string.IsNullOrEmpty(Nickname) ? FirstName : Nickname;
}

public class UniverseData
{
    public string UniverseName { get; set; }
    public string ArchetypeTitle { get; set; }
    public ModuleSettings Modules { get; set; } = new ModuleSettings();
    public Dictionary<string, EntityArchetype> Archetypes { get; set; }
    public Dictionary<string, List<StoryTemplate>> BioTemplates { get; set; }
}

public class ModuleSettings
{
    public bool ShowRarity { get; set; } = true;
    public bool UseNarrative { get; set; } = true;
    public bool UsePointPools { get; set; } = true;
}

public class EntityArchetype
{
    public List<JsonElement> FirstNames { get; set; }
    public List<JsonElement> LastNames { get; set; }
    public List<JsonElement> Nicknames { get; set; }
    public double NicknameChance { get; set; } = 0.5;
    public Dictionary<string, int> GenderOdds { get; set; }
    public List<string> StartingTags { get; set; } = new List<string>();
    public Dictionary<string, List<JsonElement>> MandatoryTraits { get; set; }
    public Dictionary<string, ProbabilityTrait> OptionalTraits { get; set; }
    public Dictionary<string, PointPool> PointPools { get; set; }
}

public class StoryTemplate
{
    public string Text { get; set; }
    public int Weight { get; set; } = 1;
    public string Requires { get; set; }
    public string MinStatName { get; set; }
    public int MinStatValue { get; set; } = 0;
}

public class ProbabilityTrait
{
    public double Chance { get; set; }
    public List<JsonElement> Options { get; set; }
}

public class PointPool
{
    public int TotalPoints { get; set; }
    public int MinPerAttribute { get; set; }
    public int MaxPerAttribute { get; set; }
    public List<string> Attributes { get; set; }
}

public class SmartOption
{
    public string Value { get; set; }
    public int Weight { get; set; } = 1;
    public string Requires { get; set; }
    public string GrantsTag { get; set; }
    public string Rarity { get; set; }
}