// Namespaces
// module = namespace
// import = C# using

// Access modifiers
// public
// private
// protected
// internal
// external

// Class modifiers
// global = C# static
// template = C# abstract
// final = C# sealed

// Struct modifiers
// immutable = C# readonly

// Field modifiers
// constant = C# const
// global = C# static
// fixed -> the variable cannot be reassigned (C# readonly)
// immutable -> makes the variable unable to have any non-immutable function/property calls
// final -> fixed + immutable (the same as calling "seal" at the end of all constructors OR when first assigned)

// Method/property modifiers
// global = C# static
// prototype = virtual
// required = abstract
// immutable -> marks the method/property as one that does not modify the class's fields

// Other keywords
// seal -> makes the variable become final
/*
module Adventure;

class Room
{
    public string Name;
    public string Description;
}

class Player
{
    public string Name;
    public Room CurrentRoom;
    
    public void MoveTo(Room room)
    {
        CurrentRoom = room;
        Console.WriteLine("You moved to: " + room.Name);
        Console.WriteLine(room.Description);
    }
}

entry int Main()
{
    // Create rooms
    Room room1 = new Room
    {
        Name = "Living Room",
        Description = "You are in the living room. There is a door to the north."
    };

    Room room2 = new Room
    {
        Name = "Kitchen",
        Description = "You are in the kitchen. There is a door to the south."
    };

    // Create player
    Player player = new Player
    {
        Name = "John",
        CurrentRoom = room1
    };

    Console.WriteLine("Welcome to the text adventure game, " + player.Name + "!");

    bool playing = true;
    while (playing)
    {
        Console.Write("Enter command: ");
        string command = Console.ReadLine();

        switch (command)
        {
            case ("north")
            {
                if (player.CurrentRoom == room1)
                {
                    player.MoveTo(room2);
                }
                else
                {
                    Console.WriteLine("You cannot go north.");
                }
            }
            case ("south")
            {
                if (player.CurrentRoom == room2)
                {
                    player.MoveTo(room1);
                }
                else
                {
                    Console.WriteLine("You cannot go south.");
                }
            }
            case ("quit")
            {
                playing = false;
                Console.WriteLine("Goodbye!");
            }
            default
            {
                Console.WriteLine("Invalid command.");
            }
        }
    }

    return 0;
}*/