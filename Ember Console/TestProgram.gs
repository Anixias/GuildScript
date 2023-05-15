import System.Console;
import System.Math;

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
	public Vector2 Location { get; set; } 
	public string Name { get; private set; }
	private Room currentRoom;
	public Room.RoomType favoriteRoom;
	
	public void MoveTo(Room room)
	{
		Location += new Vector2(1.0, 0.0);
		currentRoom = room;
		Console.WriteLine("You moved to: " + room.Name);
		Console.WriteLine(room.Description);
	}
}

entry int Main()
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
				if (player.currentRoom == room1)
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
				if (player.currentRoom == room2)
				{
					player.MoveTo(room1);
				}
				else
				{
				    var console = new Console();
					console.WriteLine("You cannot go south.");
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

	return 0i32;
}