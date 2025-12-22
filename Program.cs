Map map;
Location start;
Player player;
Game game;

Console.WriteLine("Do you want to play on a 4x4, 6x6 or 8x8 map? (Type: 4, 6 or 8)...");
string? mapSizeDecision = Console.ReadLine();
if (mapSizeDecision == "4")
{
    map = new Map(4, 4);
    start = new Location(0, 0);
    player = new Player(start);
    game = new Game(player, map, false);
    game.Map.SetRoomTypeAtLocation(start, RoomType.Entrance);
    game.Map.SetRoomTypeAtLocation(new Location(3, 2), RoomType.Fountain);
}
else if (mapSizeDecision == "6")
{
    map = new Map(6, 6);
    start = new Location(0, 0);
    player = new Player(start);
    game = new Game(player, map, false);
    game.Map.SetRoomTypeAtLocation(start, RoomType.Entrance);
    game.Map.SetRoomTypeAtLocation(new Location(1, 5), RoomType.Fountain);
}
else if (mapSizeDecision == "8")
{
    map = new Map(8, 8);
    start = new Location(0, 0);
    player = new Player(start);
    game = new Game(player, map, false);
    game.Map.SetRoomTypeAtLocation(start, RoomType.Entrance);
    game.Map.SetRoomTypeAtLocation(new Location(7, 2), RoomType.Fountain);
}
else
{
    Console.WriteLine("Invalid map size.");
    return;
}

DisplayIntro();
game.Run();
void DisplayIntro()
{
    Console.WriteLine("You enter the Cavern of Objects, a maze of rooms, in search");
    Console.WriteLine("of the Fountain of Objects.");
    Console.WriteLine("Light is visible only in the entrance, and no other light is seen anywhere in the caverns.");
    Console.WriteLine("You must navigate the Caverns with your other senses.");
    Console.WriteLine("Find the Fountain of Objects, activate it, and return to the entrance.");
}

public class Game
{
    public Player Player {  get; set; }
    public Map Map { get; set; }
    public bool IsFountainOn { get; set; }
    private readonly ISense[] Senses;

    public Game(Player player,  Map map,  bool isFountainOn)
    {
        Player = player;
        Map = map;
        IsFountainOn = isFountainOn;
        Senses = new ISense[]
            {
                new EntranceSense(),
                new FountainSense()
            };
    }
    public RoomType CurrentRoom => Map.GetRoomTypeAtLocation(Player.Location);
    public bool HasWon => CurrentRoom == RoomType.Entrance && IsFountainOn;
    public void Run()
    {
        while (!HasWon)
        {
            DisplayStatus();
            ICommand command = GetCommand();
            command.Execute(this);
        }
        Console.WriteLine("Congratulations! The Foutain of Objects has been reactivated and you have escaped with your life!");
    }
    private void DisplayStatus()
    {
        Console.WriteLine("------------------------------------------------------");
        Console.WriteLine($"You are in the room at (Row: {Player.Location.row}, Column: {Player.Location.column}).");
        foreach (ISense sense in Senses)
        {
            if (sense.CanSense(this))
            {
                sense.DisplaySense(this);
            }
        }
    }
    private ICommand GetCommand()
    {
        while (true)
        {
            Console.WriteLine("What do you want to do (Type: 'n', 'e', 's', 'w' or 'a')?");
            string? input = Console.ReadLine();

            if (input == "n") return new MoveCommand(Direction.North);
            if (input == "e") return new MoveCommand(Direction.East);
            if (input == "s") return new MoveCommand(Direction.South);
            if (input == "w") return new MoveCommand(Direction.West);
            if (input == "a") return new ActivateFountainCommand();
            else Console.WriteLine($"I did not understand '{input}'.");
        }
    }
}

public class Player
{
    public Location Location { get; set; }

    public Player(Location start)
    {
        Location = start;
    }
    public void GetLocation()
    {
        Console.WriteLine($"Player is at {Location}");
    }
}

public class Map
{
    private readonly RoomType[,] Rooms;
    private int Rows { get; set; }
    private int Cols { get; set; }

    public Map(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Rooms = new RoomType[Rows, Cols];
    }
    public bool IsOnMap(Location location)
    {   
        if (location.row >= 0 && location.row < Rooms.GetLength(0) && location.column >= 0 && location.column < Rooms.GetLength(1))
        {
            return true; 
        }
        return false;
    }
    public RoomType GetRoomTypeAtLocation(Location location)
    {
        if (IsOnMap(location))
        {
            return Rooms[location.row, location.column];
        }
        else
        {
            return RoomType.OffTheMap;
        }
    }
    public void SetRoomTypeAtLocation(Location location, RoomType type)
    {   
        Rooms[location.row, location.column] = type;
    }
}

public interface ICommand
{
    public void Execute(Game game);
}

public class MoveCommand : ICommand
{
    public Direction Direction { get; }
    public MoveCommand(Direction direction)
    {
        Direction = direction;
    }
    public void Execute(Game game)
    {
        Location currentLocation = game.Player.Location;
        Location newLocation = Direction switch
        {
            Direction.North => new Location(currentLocation.row, currentLocation.column + 1),
            Direction.South => new Location(currentLocation.row, currentLocation.column - 1),
            Direction.East => new Location(currentLocation.row + 1, currentLocation.column),
            Direction.West => new Location(currentLocation.row - 1, currentLocation.column),
            _ => currentLocation
        };
        if (game.Map.IsOnMap(newLocation))
        {
            game.Player.Location = newLocation;
        }
        else
        {
            Console.WriteLine("There is a wall there...");
        }
    }
}

public class ActivateFountainCommand : ICommand
{
    public void Execute(Game game)
    {
        RoomType currentRoom = game.Map.GetRoomTypeAtLocation(game.Player.Location);
        if (currentRoom == RoomType.Fountain)
        {
            game.IsFountainOn = true;
        }
    }
}

public class Room
{
    public int X { get; }
    public int Y { get; }

    public Room(int x, int y) 
    {
        X = x;
        Y = y;
    }
}

public interface ISense
{
    public bool CanSense(Game game);
    public void DisplaySense(Game game);
}

public class EntranceSense : ISense
{
    public bool CanSense(Game game)
    {
        RoomType PlayerLocation = game.Map.GetRoomTypeAtLocation(game.Player.Location);
        if (PlayerLocation == RoomType.Entrance)
        {
            return true;
        }
        return false;
    }
    public void DisplaySense(Game game)
    {
        Console.WriteLine("You see a light coming from the cavern entrance.");
    }
}
public class FountainSense : ISense
{
    public bool CanSense(Game game) 
    {
        RoomType PlayerLocation = game.Map.GetRoomTypeAtLocation(game.Player.Location);
        if (PlayerLocation == RoomType.Fountain)
        {
            return true;
        }
        return false;
    }
    public void DisplaySense(Game game)
    {
        if (game.IsFountainOn) 
        {
            Console.WriteLine("You hear the rushing waters from the Fountain of Objects. It has been reactivated!");
        }
        else
        {
            Console.WriteLine("You hear water dripping in this room. The Fountain of Objects is here!");
        }
    }
}

public record Location(int row, int column);

public enum Direction { North, South, West, East }
public enum RoomType { Normal, Entrance, Fountain, OffTheMap }