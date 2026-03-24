using System.Text.Json;
using System.IO;

// Random Person Generator

// --- THE SPLASH SCREEN ---
Console.Title = "Reality Engine v0.3"; // Sets the window title bar
Console.ForegroundColor = ConsoleColor.Cyan;

// ASCII Art for "REALITY ENGINE"
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
Console.WriteLine("                                Version 0.3                                ");
Console.WriteLine(" --------------------------------------------------------------------------");

Console.WriteLine("\n\n");

// A little "Loading" style prompt
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("        >> SYSTEM INITIALIZED. PRESS ANY KEY TO ENTER THE GRID <<        ");
Console.ResetColor();

Console.ReadKey(true); // 'true' hides the key the user pressed
Console.Clear();

// Main menu

bool keepRunning = true;

while (keepRunning) // This loop will keep the program running until the user chooses to exit
{
    Console.Clear();
    Console.WriteLine("Reality Engine v0.3");
    Console.WriteLine("=======================");
    Console.WriteLine("Please select from the following options:");
    Console.WriteLine("1. Generate a random person");
    Console.WriteLine("2. Instructions");
    Console.WriteLine("3. Exit");

    string userinput = Console.ReadLine();

    // TryParse attempts to convert the string to an int. 
    // If it succeeds, 'choice' holds the number. If it fails, it returns false.
    if (int.TryParse(userinput, out int choice))
    {
        switch (choice)
        {
            case 1:
            // First, we need to let the user select a universe (JSON file)
            string chosenFile = SelectUniverse();

            // If a file was selected, generate a random person from that file
            if (chosenFile != null)
                {
                    GenerateEntity(chosenFile);
                }
            break;
           
            case 2:
                ShowInstructions(); // This will show the instructions from a text file
                break;
            
            case 3:
                keepRunning = false; // This breaks the loop and ends the program
                ExitProgram();
                break;
            default:
                Console.WriteLine("Invalid choice. Please pick 1, 2 or 3.");
                Console.ReadKey();
                break;
        }
    }
    else
    {
        Console.WriteLine("That wasn't a number! Press any key to try again.");
        Console.ReadKey();
    }
}

static void ShowInstructions()
{
    string filePath = Path.Combine("Docs", "Instructions.txt");

    if (File.Exists(filePath))
    {
        string content = File.ReadAllText(filePath);
        Console.Clear();
        Console.WriteLine(content);
        Console.WriteLine("\nPress any key to return to the main menu...");
        Console.ReadKey();
    }
    else
    {
        // Helpful debug: show the full path where it's looking
        Console.WriteLine($"Error: {filePath} not found!");
        Console.WriteLine($"Full path: {Path.GetFullPath(filePath)}");
        Console.ReadKey();
    }
}

static void GenerateEntity(string filePath)
{
    string jsonString = File.ReadAllText(filePath);
    UniverseData data = JsonSerializer.Deserialize<UniverseData>(jsonString);
    Random rand = new Random();
    GeneratedEntity entity = new GeneratedEntity();

    // 1. SELECT THE CORE TYPE (The Branch)
    List<string> typeOptions = data.Archetypes.Keys.ToList();
    Console.Clear();
    Console.WriteLine($"--- {data.UniverseName} ---");
    Console.WriteLine($"{data.ArchetypeTitle}:");
    for (int i = 0; i < typeOptions.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {typeOptions[i]}");
    }

    int choice = 0;
    while (choice < 1 || choice > typeOptions.Count) { int.TryParse(Console.ReadLine(), out choice); }

    string selectedTypeName = typeOptions[choice - 1];
    EntityArchetype archetype = data.Archetypes[selectedTypeName];

    // Save the core type (e.g., "Type: Ghost")
    entity.Attributes.Add(data.ArchetypeTitle, selectedTypeName);

    // 2. ROLL NAME
    entity.Name = archetype.Names[rand.Next(archetype.Names.Count)];

    // 3. ROLL MANDATORY TRAITS (The "Must-Haves")
    foreach (var trait in archetype.MandatoryTraits)
    {
        string pickedValue = trait.Value[rand.Next(trait.Value.Count)];
        entity.Attributes.Add(trait.Key, pickedValue);
    }

    // 4. ROLL OPTIONAL TRAITS (The "Probability" Dice)
    if (archetype.OptionalTraits != null)
    {
        foreach (var trait in archetype.OptionalTraits)
        {
            // If the random number (0.0 to 1.0) is less than the chance (e.g., 0.2)
            if (rand.NextDouble() < trait.Value.Chance)
            {
                string result = trait.Value.Options[rand.Next(trait.Value.Options.Count)];
                entity.Attributes.Add(trait.Key, result);
            }
        }
    }

    // Check: Does this archetype actually have point pools defined?
    if (archetype.PointPools != null)
    {
        foreach (var poolEntry in archetype.PointPools)
        {
            string poolName = poolEntry.Key;
            PointPool pool = poolEntry.Value;

            // 1. Initialize attributes to minimum
            Dictionary<string, int> currentScores = new Dictionary<string, int>();
            int remainingPoints = pool.TotalPoints;

            foreach (var attrName in pool.Attributes)
            {
                currentScores[attrName] = pool.MinPerAttribute;
                remainingPoints -= pool.MinPerAttribute;
            }

            // 2. Randomly distribute the rest
            while (remainingPoints > 0)
            {
                string targetAttr = pool.Attributes[rand.Next(pool.Attributes.Count)];

                if (currentScores[targetAttr] < pool.MaxPerAttribute)
                {
                    currentScores[targetAttr]++;
                    remainingPoints--;
                }

                // Safety check to avoid infinite loops if pool is too small for points
                if (currentScores.Values.All(v => v == pool.MaxPerAttribute)) break;
            }

            // 3. Add to the Entity's final attribute list
            foreach (var score in currentScores)
            {
                entity.Attributes.Add($"{poolName}: {score.Key}", score.Value.ToString());
            }
        }
    }

    DisplayEntity(entity);

}

static void DisplayEntity(GeneratedEntity entity)
{
    Console.Clear();

    // --- HEADER ---
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("==================================================");
    Console.WriteLine($"   IDENTIFICATION: {entity.Name.ToUpper()}");
    Console.WriteLine("==================================================");
    Console.ResetColor();

    // 1. Display Basic Traits (Anything without a ":" in the key)
    Console.WriteLine("\n--- CORE ATTRIBUTES ---");
    foreach (var attr in entity.Attributes)
    {
        if (!attr.Key.Contains(":"))
        {
            Console.WriteLine($"{attr.Key.PadRight(20)}: {attr.Value}");
        }
    }

    // 2. Display Point Pools (Anything WITH a ":" in the key)
    // We'll group them by their prefix (the part before the colon)
    var poolGroups = entity.Attributes
        .Where(a => a.Key.Contains(":"))
        .GroupBy(a => a.Key.Split(':')[0].Trim());

    foreach (var group in poolGroups)
    {
        Console.WriteLine($"\n--- {group.Key.ToUpper()} ---");
        Console.ForegroundColor = ConsoleColor.Yellow;

        foreach (var item in group)
        {
            // Split the "PoolName: StatName" so we only show "StatName"
            string statName = item.Key.Split(':')[1].Trim();
            Console.WriteLine($"{statName.PadRight(20)}: {item.Value}");
        }
        Console.ResetColor();
    }

    Console.WriteLine("\n==================================================");
    Console.WriteLine("Press 'S' to Save or any other key to return...");
    Console.ReadLine();
}

static string SelectUniverse() // This method will allow the user to select a universe
{
    string folder = "UniverseFiles";

    // Check if the folder exists, if not, create it.
    if (!Directory.Exists(folder)) 
    { Directory.CreateDirectory(folder); }

    string[] files = Directory.GetFiles(folder, "*.json");

    if (files.Length == 0)
    {
        Console.WriteLine("Put your .json files in the '{folder}' folder!");
        Console.ReadKey();
        return null;
    }

    Console.Clear();
    Console.WriteLine("--- CHOOSE YOUR UNIVERSE ---");

    for (int i = 0; i < files.Length; i++)
    {
        Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(files[i])}");
    }
    Console.WriteLine($"{files.Length + 1}. Back");

    if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= files.Length)
    {
        return files[choice - 1];
    }
    return null;
}

static void ExitProgram() // This method will exit the program
{
    Console.WriteLine("Exiting...");
    Environment.Exit(0);
}

public class GeneratedEntity
{
    //identity
    public string Name { get; set; }
    // This holds everything: "Race", "Color", "Year", "Haunting Style", etc.
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>(); 
}

public class UniverseData
{
    public string UniverseName { get; set; }
    public string ArchetypeTitle { get; set; } // e.g., "Select Entity Type"
    public Dictionary<string, EntityArchetype> Archetypes { get; set; }
}

public class EntityArchetype
{
    public List<string> Names { get; set; }
    public Dictionary<string, List<string>> MandatoryTraits { get; set; }
    public Dictionary<string, ProbabilityTrait> OptionalTraits { get; set; }

    // This stays null if "PointPools" isn't in the JSON
    public Dictionary<string, PointPool> PointPools { get; set; }
}

public class PointPool
{
    public int TotalPoints { get; set; }
    public int MinPerAttribute { get; set; }
    public int MaxPerAttribute { get; set; }
    public List<string> Attributes { get; set; }
}

public class ProbabilityTrait
{
    public double Chance { get; set; } // 0.1 = 10%
    public List<string> Options { get; set; }
}