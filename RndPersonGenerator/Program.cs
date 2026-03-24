using System.Text.Json;
using System.IO;

// Random Person Generator

Console.WriteLine("Random Person Generator v0.1");
Console.WriteLine("=======================");
Console.WriteLine("Press any key to continue");
Console.ReadKey();
Console.Clear();

// Main menu

bool keepRunning = true;

while (keepRunning) // This loop will keep the program running until the user chooses to exit
{
    Console.Clear();
    Console.WriteLine("Random Person Generator v0.1");
    Console.WriteLine("=======================");
    Console.WriteLine("Please select from the following options:");
    Console.WriteLine("1. Generate a random person");
    Console.WriteLine("2. Exit");

    string userinput = Console.ReadLine();

    // TryParse attempts to convert the string to an int. 
    // If it succeeds, 'choice' holds the number. If it fails, it returns false.
    if (int.TryParse(userinput, out int choice))
    {
        switch (choice)
        {
            case 1:
                GenerateRandomPerson();
                break;
            case 2:
                keepRunning = false; // This breaks the loop and ends the program
                ExitProgram();
                break;
            default:
                Console.WriteLine("Invalid choice. Please pick 1 or 2.");
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



static void GenerateRandomPerson() // This method will generate a random person
{
    //1. Load the file text#
    string fileName = "Medieval.json";
    string jsonString = File.ReadAllText(fileName);

    // 2. Deserialize (Convert JSON text -> C# Object)
    UniverseData data = JsonSerializer.Deserialize<UniverseData>(jsonString);

    // 3. Create a Random object and a Person object
    Random rand = new Random();
    Person character = new Person();

    //4. "Roll" for traits using the lists from our JSON file
    character.Name = data.Names[rand.Next(data.Names.Count)];
    character.Occupation = data.Occupations[rand.Next(data.Occupations.Count)];
    character.SocialStanding = data.Standings[rand.Next(data.Standings.Count)];
    character.Secret = data.Secrets[rand.Next(data.Secrets.Count)];

    //5. Display the generated character
    Console.Clear();
    Console.WriteLine($"Genrated Character from the {data.UniverseName} universe:");
    Console.WriteLine("=======================");
    Console.WriteLine($"Name: {character.Name}");
    Console.WriteLine($"Standing: {character.SocialStanding}");
    Console.WriteLine($"Occupation: {character.Occupation}");
    Console.WriteLine($"Secret: {character.Secret}");
    Console.WriteLine("=======================");
    Console.WriteLine("\nPress any key to return to the main menu...");
    Console.ReadLine();
}

    static void ExitProgram() // This method will exit the program
{
    Console.WriteLine("Exiting...");
    Environment.Exit(0);
}

public class Person
{
    //identity
    public string Name { get; set; }
    public string SocialStanding { get; set; }

    //Background
    public string Occupation { get; set; }
    public string Hobbies { get; set; }
    public string Background { get; set; }
    public string Secret { get; set; }
    public string Motivation { get; set; }

    //Personality
    public string PersonalityTrait1 { get; set; }
}

public class UniverseData
{
    public string UniverseName { get; set; }
    public List<string> Names { get; set; }
    public List<string> Occupations { get; set; }
    public List<string> Standings { get; set; }
    public List<string> Secrets { get; set; }
}