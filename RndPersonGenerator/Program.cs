


// Random Person Generator

Console.WriteLine("Random Person Generator v0.1");
Console.WriteLine("=======================");
Console.WriteLine("Press any key to continue");
Console.ReadKey();
Console.Clear();

// Main menu

MainMenu:

Console.WriteLine("Please select from the following options");
Console.WriteLine("1. Generate a random person");
Console.WriteLine("2. Exit");

// Get user input

string userinput = Console.ReadLine();

if (userinput == "1") // Generate a random person
{
    GenerateRandomPerson();
}


if (userinput == "2") // Exit the program
{
    ExitProgram(); // Call the exit program method
}

else // Invalid input
{ 
    Console.Clear(); // Clear the console
    Console.WriteLine("Invalid input, please try again");
    Console.ReadKey(); Console.Clear(); // Clear the console
    goto MainMenu; // Go back to the main menu
}



static void GenerateRandomPerson() // This method will generate a random person
{
   
}

static void ExitProgram() // This method will exit the program
{
    Console.WriteLine("Exiting...");
    Environment.Exit(0);
}

