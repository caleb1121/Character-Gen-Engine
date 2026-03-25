using System.Text.Json;
using System.IO;
using System.Linq;

// ==========================================================
// 1. SPLASH SCREEN & INITIALIZATION
// ==========================================================

Console.Title = "Reality Engine v0.65";
Console.ForegroundColor = ConsoleColor.Cyan;

Console.WriteLine(@"
  _____  ______          _      _____ _______驰  ______ _   _  _____ _____ _   _ ______ 
 |  __ \|  ____|   /\   | |    |_   _|__   __\ \ / /  ____| \ | |/ ____|_   _| \ | |  ____|
 | |__) | |__     /  \  | |      | |    | |   \ V /| |__  |  \| | |  __  | | |  \| | |__   
 |  _  /|  __|   / /\ \ | |      | |    | |    > < |  __| | . ` | | |_ | | | | . ` |  __|  
 | | \ \| |____ / ____ \| |____ _| |_   | |   / . \| |____| |\  | |__| |_| |_| |\  | |____ 
 |_|  \_\______/_/    \_\______|_____|  |_|  /_/ \_\______|_| \_|\_____|_____|_| \_|______|
");

Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine(" --------------------------------------------------------------------------");
Console.WriteLine("                       [ UNIVERSAL CHARACTER ENGINE ]                      ");
Console.WriteLine("                                Version 0.65                               ");
Console.WriteLine(" --------------------------------------------------------------------------");
Console.WriteLine("\n\n        >> SYSTEM INITIALIZED. PRESS ANY KEY TO ENTER THE GRID <<        ");
Console.ResetColor();

Console.ReadKey(true);
Console.Clear();

// ==========================================================
// 2. GLOBAL STATE & MAIN MENU
// ==========================================================

UniverseData activeModule = null;
bool keepRunning = true;
Random rand = new Random();

while (keepRunning)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("--- REALITY ENGINE v0.65 ---");
    Console.ResetColor();

    string status = activeModule == null ? "None (Please load a module)" : activeModule.UniverseName;
    Console.WriteLine($"ACTIVE MODULE: {status}");
    Console.WriteLine("===========================");
    Console.WriteLine("1. Generate Character");
    Console.WriteLine("2. Load New Module");
    Console.WriteLine("3. Help / Instructions");
    Console.WriteLine("4. Exit");
    Console.Write("\nSelection: ");

    string input = Console.ReadLine();

    switch (input)
    {
        case "1":
            if (activeModule == null)
            {
                Console.WriteLine("\nError: No module loaded! Diverting to Load Menu...");
                System.Threading.Thread.Sleep(1000);
                LoadModuleFlow();
            }
            else { GenerateEntity(activeModule, rand); }
            break;
        case "2": LoadModuleFlow(); break;
        case "3": ShowInstructions(); break;
        case "4": keepRunning = false; ExitProgram(); break;
        default:
            Console.WriteLine("Invalid selection.");
            System.Threading.Thread.Sleep(500);
            break;
    }
}

// ==========================================================
// 3. CORE LOGIC METHODS
// ==========================================================

void LoadModuleFlow()
{
    string filePath = SelectUniverse();
    if (filePath == null) return;

    try
    {
        string jsonString = File.ReadAllText(filePath);
        activeModule = JsonSerializer.Deserialize<UniverseData>(jsonString);

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"SUCCESSFULLY LOADED: {activeModule.UniverseName}");
        Console.WriteLine("--------------------------------------------------");
        Console.ResetColor();

        Console.WriteLine($"Archetypes Found: {activeModule.Archetypes.Count}");
        foreach (var arch in activeModule.Archetypes)
        {
            Console.WriteLine($"> {arch.Key}:");
            Console.WriteLine($"  - {arch.Value.FirstNames?.Count ?? 0} First | {arch.Value.LastNames?.Count ?? 0} Last | {arch.Value.Nicknames?.Count ?? 0} Nicks");
        }

        Console.WriteLine("\nSystem Ready. Press any key to return to menu...");
        Console.ReadKey();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nCRITICAL ERROR LOADING JSON: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}

static void GenerateEntity(UniverseData data, Random rand)
{
    GeneratedEntity entity = new GeneratedEntity();
    List<string> activeTags = new List<string>();

    List<string> typeOptions = data.Archetypes.Keys.ToList();
    Console.Clear();
    Console.WriteLine($"--- {data.UniverseName} ---");
    Console.WriteLine($"{data.ArchetypeTitle}:");
    for (int i = 0; i < typeOptions.Count; i++) Console.WriteLine($"{i + 1}. {typeOptions[i]}");

    int choice = 0;
    while (choice < 1 || choice > typeOptions.Count) { int.TryParse(Console.ReadLine(), out choice); }

    string selectedTypeName = typeOptions[choice - 1];
    EntityArchetype archetype = data.Archetypes[selectedTypeName];

    if (archetype.StartingTags != null) activeTags.AddRange(archetype.StartingTags);
    entity.Attributes.Add(data.ArchetypeTitle, selectedTypeName);

    // 1. ROLL GENDER & PRONOUNS
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

    // 2. ROLL NAMES
    entity.FirstName = PickSmart(archetype.FirstNames, activeTags, rand).Value;
    entity.LastName = PickSmart(archetype.LastNames, activeTags, rand).Value;

    if (archetype.Nicknames != null && rand.NextDouble() < archetype.NicknameChance)
        entity.Nickname = PickSmart(archetype.Nicknames, activeTags, rand).Value;
    else
        entity.Nickname = "";

    // 3. ROLL MANDATORY TRAITS
    if (archetype.MandatoryTraits != null)
    {
        foreach (var trait in archetype.MandatoryTraits)
        {
            SmartOption picked = PickSmart(trait.Value, activeTags, rand);
            bool canShowRarity = data.Modules.ShowRarity && !string.IsNullOrEmpty(picked.Rarity);
            string displayValue = canShowRarity ? $"{picked.Value} [{picked.Rarity}]" : picked.Value;
            entity.Attributes.Add(trait.Key, displayValue);
        }
    }

    // 4. ROLL OPTIONAL TRAITS
    if (archetype.OptionalTraits != null)
    {
        foreach (var trait in archetype.OptionalTraits)
        {
            if (rand.NextDouble() < trait.Value.Chance)
            {
                SmartOption picked = PickSmart(trait.Value.Options, activeTags, rand);
                bool canShowRarity = data.Modules.ShowRarity && !string.IsNullOrEmpty(picked.Rarity);
                string displayValue = canShowRarity ? $"{picked.Value} [{picked.Rarity}]" : picked.Value;
                entity.Attributes.Add(trait.Key, displayValue);
            }
        }
    }

    // 5. POINT POOLS
    if (data.Modules.UsePointPools && archetype.PointPools != null)
    {
        foreach (var poolEntry in archetype.PointPools)
        {
            string poolName = poolEntry.Key;
            PointPool pool = poolEntry.Value;
            Dictionary<string, int> currentScores = new Dictionary<string, int>();
            int remainingPoints = pool.TotalPoints;

            foreach (var attrName in pool.Attributes)
            {
                currentScores[attrName] = pool.MinPerAttribute;
                remainingPoints -= pool.MinPerAttribute;
            }

            while (remainingPoints > 0)
            {
                string targetAttr = pool.Attributes[rand.Next(pool.Attributes.Count)];
                if (currentScores[targetAttr] < pool.MaxPerAttribute)
                {
                    currentScores[targetAttr]++;
                    remainingPoints--;
                }
                if (currentScores.Values.All(v => v == pool.MaxPerAttribute)) break;
            }

            foreach (var score in currentScores)
                entity.Attributes.Add($"{poolName}: {score.Key}", score.Value.ToString());
        }
    }

    entity.Tags = activeTags.Distinct().ToList();

    // 6. NARRATIVE GENERATION (Upgraded with Stat-Checks)
    if (data.Modules.UseNarrative)
        entity.Story = ConstructStory(entity, data, rand);

    DisplayEntity(entity, data.UniverseName);
}

static void AssignPronouns(GeneratedEntity entity)
{
    switch (entity.Gender.ToLower())
    {
        case "male":
            entity.Pronouns = new Dictionary<string, string> { { "Subject", "he" }, { "Object", "him" }, { "Possessive", "his" } };
            break;
        case "female":
            entity.Pronouns = new Dictionary<string, string> { { "Subject", "she" }, { "Object", "her" }, { "Possessive", "her" } };
            break;
        default:
            entity.Pronouns = new Dictionary<string, string> { { "Subject", "they" }, { "Object", "them" }, { "Possessive", "their" } };
            break;
    }
}

static string ConstructStory(GeneratedEntity entity, UniverseData data, Random rand)
{
    if (data.BioTemplates == null) return null;
    List<string> storyBits = new List<string>();

    foreach (var category in data.BioTemplates)
    {
        // FILTER: Check Tag Requirements AND Stat Thresholds
        var validTemplates = category.Value.Where(t =>
        {
            bool tagMet = string.IsNullOrEmpty(t.Requires) || entity.Tags.Contains(t.Requires);
            bool statMet = true;

            if (!string.IsNullOrEmpty(t.MinStatName))
            {
                var statEntry = entity.Attributes.FirstOrDefault(a => a.Key.EndsWith(t.MinStatName));
                if (statEntry.Key != null && int.TryParse(statEntry.Value, out int score))
                {
                    statMet = score >= t.MinStatValue;
                }
                else { statMet = false; }
            }
            return tagMet && statMet;
        }).ToList();

        if (validTemplates.Count == 0) continue;

        int totalWeight = validTemplates.Sum(t => t.Weight);
        int roll = rand.Next(0, totalWeight);
        int cursor = 0;
        string pickedText = "";

        foreach (var t in validTemplates)
        {
            cursor += t.Weight;
            if (roll < cursor) { pickedText = t.Text; break; }
        }

        if (string.IsNullOrEmpty(pickedText)) pickedText = validTemplates[0].Text;

        // Placeholders
        pickedText = pickedText.Replace("{First}", entity.FirstName)
                               .Replace("{Last}", entity.LastName)
                               .Replace("{Nick}", entity.NicknameOrFirst)
                               .Replace("{Full}", entity.FullName)
                               .Replace("{Gender}", entity.Gender);

        foreach (var p in entity.Pronouns) { pickedText = pickedText.Replace($"{{{p.Key}}}", p.Value); }
        foreach (var attr in entity.Attributes) { pickedText = pickedText.Replace($"{{{attr.Key}}}", attr.Value); }

        storyBits.Add(pickedText);
    }
    return string.Join(" ", storyBits);
}

static void DisplayEntity(GeneratedEntity entity, string universeName)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==================================================");
    Console.WriteLine($"   IDENTIFICATION: {entity.FullName.ToUpper()}");
    Console.WriteLine($"   GENDER: {entity.Gender}");
    if (entity.Tags.Count > 0)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"   TAGS: {string.Join(", ", entity.Tags)}");
    }
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==================================================");
    Console.ResetColor();

    if (!string.IsNullOrEmpty(entity.Story))
    {
        Console.WriteLine("\n--- DATA LOG ---");
        Console.WriteLine(entity.Story);
        Console.WriteLine("----------------");
    }

    Console.WriteLine("\n--- CORE ATTRIBUTES ---");
    foreach (var attr in entity.Attributes.Where(a => !a.Key.Contains(":")))
        Console.WriteLine($"{attr.Key.PadRight(20)}: {attr.Value}");

    var poolGroups = entity.Attributes.Where(a => a.Key.Contains(":")).GroupBy(a => a.Key.Split(':')[0].Trim());
    foreach (var group in poolGroups)
    {
        Console.WriteLine($"\n--- {group.Key.ToUpper()} ---");
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var item in group)
            Console.WriteLine($"{item.Key.Split(':')[1].Trim().PadRight(20)}: {item.Value}");
        Console.ResetColor();
    }

    Console.WriteLine("\n==================================================");
    Console.WriteLine("Press 'S' to Save or any other key to return...");
    ConsoleKeyInfo key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.S) { SaveEntityToFile(entity, universeName); }
}

// ==========================================================
// 4. PERSISTENCE & UTILITY
// ==========================================================

static void SaveEntityToFile(GeneratedEntity entity, string universeName)
{
    string baseFolder = "Saves";
    string universeFolder = Path.Combine(baseFolder, universeName.Replace(":", ""));
    if (!Directory.Exists(universeFolder)) Directory.CreateDirectory(universeFolder);

    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string fileName = $"{entity.FullName.Replace(" ", "_")}_{timestamp}.txt";
    string fullPath = Path.Combine(universeFolder, fileName);

    using (StreamWriter writer = new StreamWriter(fullPath))
    {
        writer.WriteLine("==================================================");
        writer.WriteLine($"   CHARACTER RECORD: {entity.FullName.ToUpper()}");
        writer.WriteLine($"   GENDER: {entity.Gender}");
        writer.WriteLine($"   TAGS: {string.Join(", ", entity.Tags)}");
        writer.WriteLine($"   GENERATED: {DateTime.Now}");
        writer.WriteLine("==================================================");
        if (!string.IsNullOrEmpty(entity.Story)) { writer.WriteLine($"\n[ BIOGRAPHY ]\n{entity.Story}\n"); }
        writer.WriteLine("[ CORE ATTRIBUTES ]");
        foreach (var attr in entity.Attributes.Where(a => !a.Key.Contains(":")))
            writer.WriteLine($"{attr.Key.PadRight(20)}: {attr.Value}");

        var poolGroups = entity.Attributes.Where(a => a.Key.Contains(":")).GroupBy(a => a.Key.Split(':')[0].Trim());
        foreach (var group in poolGroups)
        {
            writer.WriteLine($"\n[ {group.Key.ToUpper()} ]");
            foreach (var item in group)
                writer.WriteLine($"{item.Key.Split(':')[1].Trim().PadRight(20)}: {item.Value}");
        }
    }
    Console.WriteLine($"\n[SUCCESS] Saved to: {fullPath}");
    System.Threading.Thread.Sleep(1500);
}

static string SelectUniverse()
{
    string folder = "UniverseFiles";
    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
    string[] files = Directory.GetFiles(folder, "*.json");
    if (files.Length == 0) { Console.WriteLine("\nNo modules found."); Console.ReadKey(); return null; }
    Console.Clear();
    for (int i = 0; i < files.Length; i++) Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(files[i])}");
    if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= files.Length) return files[choice - 1];
    return null;
}

static void ShowInstructions()
{
    string folder = "Docs";
    if (!Directory.Exists(folder))
    {
        Console.WriteLine("\n[ERROR] Docs folder not found. Please create a 'Docs' folder.");
        Console.ReadKey();
        return;
    }

    // Sort files alphabetically to ensure 00_Start, 01_Level1, etc.
    string[] files = Directory.GetFiles(folder, "*.txt").OrderBy(f => f).ToArray();

    bool stayInHelp = true;
    while (stayInHelp)
    {
        Console.Clear(); // Clear before redrawing menu
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================");
        Console.WriteLine("          REALITY ENGINE: HELP LIBRARY            ");
        Console.WriteLine("==================================================");
        Console.ResetColor();

        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileNameWithoutExtension(files[i]).Replace("_", " ");
            Console.WriteLine($"{i + 1}. {fileName}");
        }
        Console.WriteLine($"{files.Length + 1}. [ Back to Main Menu ]");

        Console.Write("\nSelect a module to study: ");
        string input = Console.ReadLine();

        if (int.TryParse(input, out int choice))
        {
            if (choice > 0 && choice <= files.Length)
            {
                // THE FIX: Explicitly clear the screen BEFORE printing the new file
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Gray;

                // Read and display the specific file
                string content = File.ReadAllText(files[choice - 1]);
                Console.WriteLine(content);

                Console.ResetColor();
                Console.WriteLine("\n--------------------------------------------------");
                Console.WriteLine(">> Press any key to return to the Help Library...");
                Console.ReadKey(true); // 'true' prevents the key character from printing
            }
            else if (choice == files.Length + 1)
            {
                stayInHelp = false;
            }
        }
    }
}

static void ExitProgram() { Environment.Exit(0); }

// ==========================================================
// 5. SMART RESOLUTION HELPERS
// ==========================================================

static SmartOption ResolveElement(JsonElement element)
{
    if (element.ValueKind == JsonValueKind.String)
        return new SmartOption { Value = element.GetString(), Weight = 1 };

    if (element.ValueKind == JsonValueKind.Object)
    {
        return new SmartOption
        {
            Value = element.TryGetProperty("Value", out var v) ? v.GetString() : "Unknown",
            Weight = element.TryGetProperty("Weight", out var w) ? w.GetInt32() : 1,
            Requires = element.TryGetProperty("Requires", out var r) ? r.GetString() : null,
            GrantsTag = element.TryGetProperty("GrantsTag", out var g) ? g.GetString() : null,
            Rarity = element.TryGetProperty("Rarity", out var rar) ? rar.GetString() : null
        };
    }
    return new SmartOption { Value = "Error", Weight = 1 };
}

static SmartOption PickSmart(List<JsonElement> elements, List<string> activeTags, Random rand)
{
    if (elements == null || elements.Count == 0) return new SmartOption { Value = "N/A" };
    var options = elements.Select(e => ResolveElement(e)).ToList();
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
                activeTags.Add(opt.GrantsTag);
            return opt;
        }
    }
    return validOptions[0];
}

// ==========================================================
// 6. TYPE DECLARATIONS (Classes)
// ==========================================================

public class GeneratedEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Nickname { get; set; }
    public string Gender { get; set; }
    public Dictionary<string, string> Pronouns { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    public List<string> Tags { get; set; } = new List<string>();
    public string Story { get; set; }

    public string FullName => string.IsNullOrEmpty(Nickname) ? $"{FirstName} {LastName}" : $"{FirstName} '{Nickname}' {LastName}";
    public string NicknameOrFirst => string.IsNullOrEmpty(Nickname) ? FirstName : Nickname;
}

public class UniverseData
{
    public string UniverseName { get; set; }
    public ModuleSettings Modules { get; set; } = new ModuleSettings();
    public string ArchetypeTitle { get; set; }
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