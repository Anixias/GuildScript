import System.Console;

module Adventure;

class Room
{
	public string Name { get; set; }
	public string Description { get; set; }
	public RoomType Type { get; set; }
	
	public enum int RoomType
	{
		Trap,
		Treasure,
		Boss,
		Normal,
	}
}

class Player
{
	public string Name { get; private set; }
	private Room currentRoom;
	public Room.RoomType favoriteRoom;
	
	public void MoveTo(Room room)
	{
		currentRoom = room;
		Console.WriteLine("You moved to: " + room.Name);
		Console.WriteLine(room.Description);
	}
}

entry int Main(int arg1, string arg2, Player arg3)
{
	// Create rooms
	var room1 = new Room
	{
		Name = "Living Room",
		Description = "You are in the living room. There is a door to the north."
	};

	var room2 = new Room
	{
		Name = "Kitchen",
		Description = "You are in the kitchen. There is a door to the south."
	};

	// Create player
	var player = new Player
	{
		Name = "Collin",
		currentRoom = room1
	};

	Console.WriteLine("Welcome to the text adventure game, " + player.Name + "!");

	var playing = true;
	while (playing)
	{
		Console.Write("Enter command: ");
		var command = Console.ReadLine();

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
}