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

internal class GraphicsConfig
{
    public char Undefined { get; set; } = '░';
    public char Player { get; set; } = '⚔';
    public char Friend { get; set; } = '☺';
    public char Enemy { get; set; } = '☠';
    public char Obstacle { get; set; } = '█';
    public char Passage { get; set; } = '·';
    public char PathHorizontal { get; set; } = '═';
    public char PathVertical { get; set; } = '║';
    public char PathCross { get; set; } = '╬';
    public char PathUpRight { get; set; } = '╚';
    public char PathUpLeft { get; set; } = '╝';
    public char PathDownRight { get; set; } = '╔';
    public char PathDownLeft { get; set; } = '╗';
    public char PathTUp { get; set; } = '╩';
    public char PathTDown { get; set; } = '╦';
    public char PathTLeft { get; set; } = '╣';
    public char PathTRight { get; set; } = '╠';
    public char BorderTopLeft { get; set; } = '┌';
    public char BorderTopRight { get; set; } = '┐';
    public char BorderBottomLeft { get; set; } = '└';
    public char BorderBottomRight { get; set; } = '┘';
    public char BorderHorizontal { get; set; } = '─';
    public char BorderVertical { get; set; } = '│';
}

internal class GameMap
{
    private readonly GraphicsConfig graphics;
    private int[][] LevelData { get; set; }
    private static readonly Random random = new();
    private (int, int) playerPlacement;

    public GameMap((int, int) size, GraphicsConfig? graphicsConfig = null)
    {
        (int width, int height) = size;
        LevelData = new int[height][];
        for (int r = 0; r < height; r++)
        {
            LevelData[r] = new int[width];
            for (int c = 0; c < width; c++)
            {
                LevelData[r][c] = (int)MapCodes.Undefined;
            }
        }

        graphics = graphicsConfig ?? new GraphicsConfig();
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

        var borderTopBottom = new string(graphics.BorderHorizontal, LevelData[0].Length);
        var borderSide = graphics.BorderVertical.ToString();

        sb.AppendLine(graphics.BorderTopLeft + borderTopBottom + graphics.BorderTopRight);
        for (int r = 0; r < LevelData.Length; r++)
        {
            sb.Append(borderSide);
            for (int c = 0; c < LevelData[0].Length; c++)
            {
                switch (LevelData[r][c])
                {
                    case (int)MapCodes.Undefined:
                        sb.Append(graphics.Undefined);
                        break;
                    case (int)MapCodes.Player:
                        sb.Append(graphics.Player);
                        break;
                    case (int)MapCodes.Friend:
                        sb.Append(graphics.Friend);
                        break;
                    case (int)MapCodes.Enemy:
                        sb.Append(graphics.Enemy);
                        break;
                    case (int)MapCodes.Path:
                    case (int)MapCodes.SlowPath:
                    case (int)MapCodes.FastPath:
                        sb.Append(GetPathChar(r, c));
                        break;
                    case (int)MapCodes.Obstacle:
                        sb.Append(graphics.Obstacle);
                        break;
                    case (int)MapCodes.Passage:
                        sb.Append(graphics.Passage);
                        break;
                    default:
                        throw new NotImplementedException($"I don't know how to render the code {LevelData[r][c]}.");
                }
            }
            sb.Append(borderSide);
            sb.AppendLine();
        }
        sb.AppendLine(graphics.BorderBottomLeft + borderTopBottom + graphics.BorderBottomRight);
        return sb.ToString();
    }

    private char GetPathChar(int r, int c)
    {
        bool up = r > 0 && IsPath(LevelData[r - 1][c]);
        bool down = r < LevelData.Length - 1 && IsPath(LevelData[r + 1][c]);
        bool left = c > 0 && IsPath(LevelData[r][c - 1]);
        bool right = c < LevelData[0].Length - 1 && IsPath(LevelData[r][c + 1]);

        if (up && down && left && right) return graphics.PathCross;
        if (up && down && left) return graphics.PathTLeft;
        if (up && down && right) return graphics.PathTRight;
        if (left && right && up) return graphics.PathTUp;
        if (left && right && down) return graphics.PathTDown;
        if (up && down) return graphics.PathVertical;
        if (left && right) return graphics.PathHorizontal;
        if (up && left) return graphics.PathUpLeft;
        if (up && right) return graphics.PathUpRight;
        if (down && left) return graphics.PathDownLeft;
        if (down && right) return graphics.PathDownRight;
        if (up || down) return graphics.PathVertical;
        if (left || right) return graphics.PathHorizontal;
        return graphics.Undefined;
    }

    private static bool IsPath(int code)
    {
        return code == (int)MapCodes.Path || code == (int)MapCodes.SlowPath || code == (int)MapCodes.FastPath;
    }



    public void Save(string fileName)
    {
        using var writer = new StreamWriter(fileName);
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
    private readonly GameMap gameMap = map;

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