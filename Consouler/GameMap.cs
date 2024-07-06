using System.Text;

namespace Consouler;

internal enum MapCodes
{
    Undefined = -1,
    Player = 0,
    Friend,
    Enemy,
    Path,
    SlowPath,
    FastPath,
    Obstacle,
    Passage
}

internal class GameMap
{
    private int[][] LevelData { get; set; }
    private static readonly Random random = new();
    private (int, int) playerPlacement;

    public GameMap(int width, int height = 30)
    {
        LevelData = new int[height][];
        for (int r = 0; r < height; r++)
        {
            LevelData[r] = new int[width];
            for (int c = 0; c < width; c++)
            {
                LevelData[r][c] = (int)MapCodes.Undefined;
            }
        }
    }

    public bool UpdateMapData((int x, int y) coordinates, MapCodes code)
    {
        if (!IsValidPosition(coordinates))
        {
            return false;
        }

        LevelData[coordinates.x][coordinates.y] = (int)code;
        return LevelData[coordinates.x][coordinates.y] == (int)code;
    }

    public bool IsValidPosition((int x, int y) coordinates)
    {
        return !(coordinates.x < 0 || coordinates.y < 0) && !(coordinates.x >= LevelData.Length || coordinates.y >= LevelData[0].Length);
    }

    public static (int x, int y) CalculateNewPosition((int x, int y) currentPosition, ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.K or ConsoleKey.W or ConsoleKey.UpArrow => (currentPosition.x - 1, currentPosition.y),
            ConsoleKey.J or ConsoleKey.S or ConsoleKey.DownArrow => (currentPosition.x + 1, currentPosition.y),
            ConsoleKey.H or ConsoleKey.A or ConsoleKey.LeftArrow => (currentPosition.x, currentPosition.y - 1),
            ConsoleKey.L or ConsoleKey.D or ConsoleKey.RightArrow => (currentPosition.x, currentPosition.y + 1),
            _ => currentPosition,
        };
    }

    public (int, int) GetPlayerInit() => playerPlacement;


    public void GenerateRandomMap()
    {
        GenerateMaze();
        playerPlacement = PlacePlayer();
        PlaceFriendsAndEnemies();
        PlaceObstacles();
    }

    private void GenerateMaze()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;

        var stack = new Stack<(int x, int y)>();
        stack.Push((1, 1));
        LevelData[1][1] = (int)MapCodes.Path;

        while (stack.Count > 0)
        {
            var cell = stack.Pop();
            var neighbors = GetNeighbors(cell, width, height);

            if (neighbors.Count > 0)
            {
                stack.Push(cell);
                var next = neighbors[random.Next(neighbors.Count)];
                LevelData[next.x][next.y] = (int)MapCodes.Path;
                // Mark the wall between as path
                LevelData[(cell.x + next.x) / 2][(cell.y + next.y) / 2] = (int)MapCodes.Path;
                stack.Push(next);
            }
        }
    }

    private List<(int x, int y)> GetNeighbors((int x, int y) cell, int width, int height)
    {
        var neighbors = new List<(int x, int y)>();
        var directions = new List<(int dx, int dy)>
            {
                (-2, 0), (2, 0), (0, -2), (0, 2)
            };

        foreach (var (dx, dy) in directions)
        {
            int nx = cell.x + dx;
            int ny = cell.y + dy;

            if (nx > 0 && ny > 0 && nx < height && ny < width && LevelData[nx][ny] == (int)MapCodes.Undefined)
            {
                neighbors.Add((nx, ny));
            }
        }

        return neighbors;
    }

    private (int, int) PlacePlayer()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;

        while (true)
        {
            int x = random.Next(1, height);
            int y = random.Next(1, width);

            if (LevelData[x][y] == (int)MapCodes.Path)
            {
                LevelData[x][y] = (int)MapCodes.Player;
                return (x, y);
            }
        }
    }

    private void PlaceFriendsAndEnemies()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;

        for (int i = 0; i < 10; i++)
        {
            while (true)
            {
                int x = random.Next(1, height);
                int y = random.Next(1, width);

                if (LevelData[x][y] == (int)MapCodes.Path)
                {
                    LevelData[x][y] = (int)MapCodes.Friend;
                    break;
                }
            }

            while (true)
            {
                int x = random.Next(1, height);
                int y = random.Next(1, width);

                if (LevelData[x][y] == (int)MapCodes.Path)
                {
                    LevelData[x][y] = (int)MapCodes.Enemy;
                    break;
                }
            }
        }
    }

    private void PlaceObstacles()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;

        for (int i = 0; i < 50; i++)
        {
            while (true)
            {
                int x = random.Next(1, height);
                int y = random.Next(1, width);

                if (LevelData[x][y] == (int)MapCodes.Path)
                {
                    LevelData[x][y] = (int)MapCodes.Obstacle;
                    break;
                }
            }
        }
    }


    public string Render()
    {
        var sb = new StringBuilder();

        var borderTopBottom = new string('-', LevelData[0].Length);
        var borderSide = "|";

        sb.AppendLine('/' + borderTopBottom + '\\');
        // draw map
        for (int r = 0; r < LevelData.Length; r++)
        {
            sb.Append(borderSide);
            for (int c = 0; c < LevelData[0].Length; c++)
            {
                switch (LevelData[r][c])
                {
                    case (int)MapCodes.Undefined:
                        sb.Append(' '); // Empty space
                        break;
                    case (int)MapCodes.Player:
                        sb.Append('X'); // Player
                        break;
                    case (int)MapCodes.Friend:
                        sb.Append('f'); // Friend
                        break;
                    case (int)MapCodes.Enemy:
                        sb.Append('3'); // Enemy
                        break;
                    case (int)MapCodes.Path:
                        sb.Append('='); // Path
                        break;
                    case (int)MapCodes.SlowPath:
                        sb.Append('~'); // Slow Path
                        break;
                    case (int)MapCodes.FastPath:
                        sb.Append('>'); // Fast Path
                        break;
                    case (int)MapCodes.Obstacle:
                        sb.Append('#'); // Obstacle
                        break;
                    case (int)MapCodes.Passage:
                        sb.Append('.'); // Passage
                        break;
                    default:
                        throw new NotImplementedException($"I don't know how to render the code {LevelData[r][c]}.");
                }
            }
            sb.Append(borderSide);
            sb.AppendLine();
        }
        sb.AppendLine('\\' + borderTopBottom + '/');
        return sb.ToString();
    }


    public void Save(string fileName)
    {
        using (var writer = new StreamWriter(fileName))
        {
            for (int r = 0; r < LevelData.Length; r++)
            {
                for (int c = 0; c < LevelData[0].Length; c++)
                {
                    writer.Write(LevelData[r][c]);
                    if (c < LevelData[0].Length - 1)
                    {
                        writer.Write(",");
                    }
                }
                writer.WriteLine();
            }
        }
    }

    public void Load(string fileName)
    {
        var lines = File.ReadAllLines(fileName);
        for (int r = 0; r < lines.Length; r++)
        {
            var cells = lines[r].Split(',');
            for (int c = 0; c < cells.Length; c++)
            {
                LevelData[r][c] = int.Parse(cells[c]);
            }
        }
    }
}

internal class Editor(GameMap map)
{
    private (int x, int y) cursorPosition = (0, 0);
    private GameMap gameMap = map;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(gameMap.Render());
            var menu = """"
                Editor Mode: Use arrow keys to move, 1-8 to place tiles, S to save, Q to quit
                1 => Player,
                2 => Friend,
                3 => Enemy,
                4 => Path,
                5 => SlowPath,
                6 => FastPath,
                7 => Obstacle,
                8 => Passage,
                """";
            Console.WriteLine(menu);
            Console.SetCursorPosition(cursorPosition.y + 1, cursorPosition.x + 1);
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Q)
            {
                break;
            }
            else if (key == ConsoleKey.S)
            {
                Console.WriteLine("Enter filename to save: ");
                var fileName = Console.ReadLine();
                fileName ??= $"map-{new DateTime()}";
                gameMap.Save(fileName);
            }
            else
            {
                MoveCursor(key);
                PlaceTile(key);
            }
        }
    }

    private void MoveCursor(ConsoleKey key)
    {
        var newPosition = GameMap.CalculateNewPosition(cursorPosition, key);
        if (gameMap.IsValidPosition(newPosition))
        {
            cursorPosition = newPosition;
        }
    }

    private void PlaceTile(ConsoleKey key)
    {
        MapCodes? tile = key switch
        {
            ConsoleKey.D1 => MapCodes.Player,
            ConsoleKey.D2 => MapCodes.Friend,
            ConsoleKey.D3 => MapCodes.Enemy,
            ConsoleKey.D4 => MapCodes.Path,
            ConsoleKey.D5 => MapCodes.SlowPath,
            ConsoleKey.D6 => MapCodes.FastPath,
            ConsoleKey.D7 => MapCodes.Obstacle,
            ConsoleKey.D8 => MapCodes.Passage,
            _ => (MapCodes?)null
        };

        if (tile.HasValue)
        {
            gameMap.UpdateMapData(cursorPosition, tile.Value);
        }
    }
}