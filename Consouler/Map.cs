using System.Text;

namespace Consouler;

internal enum MapCodes
{
    Undefined,
    Player,
    Friend,
    Enemy,
    Path,
    SlowPath,
    FastPath,
    Obstacle,
    Exit,
    Breadcrumb
}

internal class GraphicsConfig
{
    public char Undefined { get; set; } = ' ';
    public char Path { get; set; } = '░';
    public char SlowPath { get; set; } = '░';
    public char FastPath { get; set; } = '▒';
    public char Player { get; set; } = '⚔';
    public char Friend { get; set; } = '☺';
    public char Enemy { get; set; } = '☠';
    public char Obstacle { get; set; } = '█';
    public char Exit { get; set; } = '·';
    public char Breadcrumb { get; set; } = '•';
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

    public GraphicsConfig()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

}

internal class Map
{
    private readonly GraphicsConfig graphics;
    private int[][] LevelData { get; set; }
    private static readonly Random random = new();
    private readonly List<(int, int)> exits = [];

    public Map((int, int) size, GraphicsConfig? graphicsConfig = null)
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
        return !(coordinates.x < 0 || coordinates.y < 0) &&
               !(coordinates.x >= LevelData.Length || coordinates.y >= LevelData[0].Length) &&
               LevelData[coordinates.x][coordinates.y] != (int)MapCodes.Obstacle;
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


    public void GenerateRandomMap()
    {
        ClearMap();
        GenerateMaze();
        ConnectRooms();
        PlacePlayer();
        PlaceFriendsAndEnemies();
        PlaceExits();
        PlaceBreadcrumbs();
    }

    private void ClearMap()
    {
        for (int r = 0; r < LevelData.Length; r++)
        {
            for (int c = 0; c < LevelData[r].Length; c++)
            {
                LevelData[r][c] = (int)MapCodes.Obstacle;
            }
        }
    }

    private void GenerateMaze()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;

        var stack = new Stack<(int x, int y)>();
        (int x, int y) start = (1, 1);
        stack.Push(start);
        LevelData[start.x][start.y] = (int)MapCodes.Undefined;

        while (stack.Count > 0)
        {
            var cell = stack.Pop();
            var neighbors = GetUnvisitedNeighbors(cell, width, height);

            foreach (var next in neighbors.OrderBy(_ => random.Next()))
            {
                LevelData[next.x][next.y] = (int)MapCodes.Undefined;
                LevelData[(cell.x + next.x) / 2][(cell.y + next.y) / 2] = (int)MapCodes.Undefined;
                stack.Push(next);
            }
        }
    }

    private List<(int x, int y)> GetUnvisitedNeighbors((int x, int y) cell, int width, int height)
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

            if (nx > 0 && ny > 0 && nx < height - 1 && ny < width - 1 && LevelData[nx][ny] == (int)MapCodes.Obstacle)
            {
                neighbors.Add((nx, ny));
            }
        }

        return neighbors;
    }

    private void ConnectRooms()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;

        for (int r = 1; r < height - 1; r += 2)
        {
            for (int c = 1; c < width - 1; c += 2)
            {
                if (random.Next(100) < 20) // 20% chance to create an additional connection
                {
                    int direction = random.Next(2);
                    if (direction == 0 && c + 1 < width - 1)
                    {
                        LevelData[r][c + 1] = (int)MapCodes.Undefined;
                    }
                    else if (direction == 1 && r + 1 < height - 1)
                    {
                        LevelData[r + 1][c] = (int)MapCodes.Undefined;
                    }
                }
            }
        }
    }

    private void PlacePlayer()
    {
        (int x, int y) position;
        do
        {
            position = (random.Next(1, LevelData.Length - 1), random.Next(1, LevelData[0].Length - 1));
        } while (LevelData[position.x][position.y] != (int)MapCodes.Undefined);

        LevelData[position.x][position.y] = (int)MapCodes.Player;
    }

    private void PlaceFriendsAndEnemies()
    {
        int width = LevelData[0].Length;
        int height = LevelData.Length;
        int count = (int)(width * height * 0.05);

        for (int i = 0; i < count; i++)
        {
            PlaceEntity(MapCodes.Friend);
            PlaceEntity(MapCodes.Enemy);
        }
    }

    private void PlaceEntity(MapCodes entityType)
    {
        (int x, int y) position;
        do
        {
            position = (random.Next(1, LevelData.Length - 1), random.Next(1, LevelData[0].Length - 1));
        } while (LevelData[position.x][position.y] != (int)MapCodes.Undefined &&
                 LevelData[position.x][position.y] != (int)MapCodes.Path &&
                 LevelData[position.x][position.y] != (int)MapCodes.SlowPath &&
                 LevelData[position.x][position.y] != (int)MapCodes.FastPath);

        LevelData[position.x][position.y] = (int)entityType;
    }


    private void PlaceExits()
    {
        exits.Clear();
        PlaceExit(0, FindValidExitPosition(0, true)); // Top
        PlaceExit(LevelData.Length - 1, FindValidExitPosition(LevelData.Length - 1, true)); // Bottom
        PlaceExit(FindValidExitPosition(0, false), 0); // Left
        PlaceExit(FindValidExitPosition(LevelData[0].Length - 1, false), LevelData[0].Length - 1); // Right
    }

    private int FindValidExitPosition(int fixedCoordinate, bool isHorizontal)
    {
        List<int> validPositions = new List<int>();
        int length = isHorizontal ? LevelData[0].Length : LevelData.Length;

        for (int i = 1; i < length - 1; i++)
        {
            int x = isHorizontal ? fixedCoordinate : i;
            int y = isHorizontal ? i : fixedCoordinate;

            if (HasAdjacentUndefinedTile(x, y))
            {
                validPositions.Add(i);
            }
        }

        return validPositions.Count > 0 ? validPositions[random.Next(validPositions.Count)] : -1;
    }

    private bool HasAdjacentUndefinedTile(int x, int y)
    {
        int[][] directions = new int[][]
        {
        new int[] {-1, 0}, new int[] {1, 0}, new int[] {0, -1}, new int[] {0, 1}
        };

        foreach (var dir in directions)
        {
            int newX = x + dir[0];
            int newY = y + dir[1];
            if (IsValidPosition((newX, newY)) && LevelData[newX][newY] == (int)MapCodes.Undefined)
            {
                return true;
            }
        }
        return false;
    }

    private void PlaceExit(int x, int y)
    {
        if (x >= 0 && x < LevelData.Length && y >= 0 && y < LevelData[0].Length)
        {
            LevelData[x][y] = (int)MapCodes.Exit;
            exits.Add((x, y));

            int[][] directions = [[-1, 0], [1, 0], [0, -1], [0, 1]];

            foreach (var dir in directions)
            {
                int newX = x + dir[0];
                int newY = y + dir[1];
                if (IsValidPosition((newX, newY)) && LevelData[newX][newY] == (int)MapCodes.Obstacle)
                {
                    LevelData[newX][newY] = (int)MapCodes.Undefined;
                    break;
                }
            }
        }
    }

    public void PlaceBreadcrumbs(bool isNearest = false)
    {
        if (isNearest) PlaceBreadcrumbsToNearestExit();
        else PlaceBreadcrumbsToAllExits();
    }


    private void PlaceBreadcrumbsToNearestExit()
    {
        var playerPosition = GetPlayerPosition();
        if (playerPosition == (-1, -1)) return;

        var pathToExit = FindPathToNearestExit(playerPosition);

        foreach (var position in pathToExit)
        {
            if (LevelData[position.Item1][position.Item2] == (int)MapCodes.Undefined)
            {
                LevelData[position.Item1][position.Item2] = (int)MapCodes.Breadcrumb;
            }
        }
    }

    private void PlaceBreadcrumbsToAllExits()
    {
        var playerPosition = GetPlayerPosition();
        if (playerPosition == (-1, -1)) return;

        foreach (var exit in exits)
        {
            var pathToExit = FindPathToExit(playerPosition, exit);

            foreach (var position in pathToExit)
            {
                if (LevelData[position.Item1][position.Item2] == (int)MapCodes.Undefined)
                {
                    LevelData[position.Item1][position.Item2] = (int)MapCodes.Breadcrumb;
                }
            }
        }
    }

    private List<(int, int)> FindPathToExit((int x, int y) start, (int x, int y) exit)
    {
        var queue = new Queue<(int x, int y)>();
        var visited = new Dictionary<(int, int), (int, int)>();
        queue.Enqueue(start);
        visited[start] = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == exit)
            {
                return ReconstructPath(visited, start, current);
            }

            foreach (var neighbor in GetValidNeighbors(current))
            {
                if (!visited.ContainsKey(neighbor))
                {
                    visited[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return [];
    }

    public (int, int) GetPlayerPosition()
    {
        for (int r = 0; r < LevelData.Length; r++)
        {
            for (int c = 0; c < LevelData[r].Length; c++)
            {
                if (LevelData[r][c] == (int)MapCodes.Player)
                {
                    return (r, c);
                }
            }
        }
        return (-1, -1);
    }

    private List<(int, int)> FindPathToNearestExit((int x, int y) start)
    {
        var queue = new Queue<(int x, int y)>();
        var visited = new Dictionary<(int, int), (int, int)>();
        queue.Enqueue(start);
        visited[start] = start;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (IsExit(current))
            {
                return ReconstructPath(visited, start, current);
            }

            foreach (var neighbor in GetValidNeighbors(current))
            {
                if (!visited.ContainsKey(neighbor))
                {
                    visited[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return [];
    }

    private static List<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> visited, (int x, int y) start, (int x, int y) end)
    {
        var path = new List<(int, int)>();
        var current = end;

        while (current != start)
        {
            path.Add(current);
            current = visited[current];
        }

        path.Reverse();
        return path;
    }

    private List<(int, int)> GetValidNeighbors((int x, int y) cell)
    {
        var neighbors = new List<(int, int)>();
        var directions = new List<(int dx, int dy)>
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        foreach (var (dx, dy) in directions)
        {
            int nx = cell.x + dx;
            int ny = cell.y + dy;

            if (IsValidPosition((nx, ny)) && LevelData[nx][ny] != (int)MapCodes.Obstacle)
            {
                neighbors.Add((nx, ny));
            }
        }

        return neighbors;
    }

    public bool IsExit((int x, int y) position) => exits.Contains(position);


    public string Render(bool showBreadcrumbs = false)
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
                var code = (MapCodes)LevelData[r][c];
                switch (code)
                {
                    case MapCodes.Undefined:
                        sb.Append(graphics.Undefined);
                        break;
                    case MapCodes.Player:
                        sb.Append(graphics.Player);
                        break;
                    case MapCodes.Friend:
                        sb.Append(graphics.Friend);
                        break;
                    case MapCodes.Enemy:
                        sb.Append(graphics.Enemy);
                        break;
                    case MapCodes.Path:
                    case MapCodes.SlowPath:
                    case MapCodes.FastPath:
                        sb.Append(GetPathChar(r, c));
                        break;
                    case MapCodes.Obstacle:
                        sb.Append(graphics.Obstacle);
                        break;
                    case MapCodes.Exit:
                        sb.Append(graphics.Exit);
                        break;
                    case MapCodes.Breadcrumb:
                        sb.Append(showBreadcrumbs ? graphics.Breadcrumb : graphics.Undefined);
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


class Editor(Map map)
{
    private (int x, int y) cursorPosition = (0, 0);
    private readonly Map gameMap = map;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(gameMap.Render());
            DisplayEditorMenu();
            Console.SetCursorPosition(cursorPosition.y + 1, cursorPosition.x + 1);

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Q)
            {
                break;
            }
            else if (key == ConsoleKey.S)
            {
                SaveMap();
            }
            else
            {
                HandleEditorInput(key);
            }
        }
    }

    private static void DisplayEditorMenu()
    {
        Console.WriteLine("Editor Mode: Use arrow keys to move, 1-8 to place tiles, S to save, Q to quit");
        Console.WriteLine("1 => Player");
        Console.WriteLine("2 => Friend");
        Console.WriteLine("3 => Enemy");
        Console.WriteLine("4 => Path");
        Console.WriteLine("5 => SlowPath");
        Console.WriteLine("6 => FastPath");
        Console.WriteLine("7 => Obstacle");
        Console.WriteLine("8 => Exit");
    }

    private void SaveMap()
    {
        Console.Write("Enter filename to save: ");
        var fileName = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"map-{DateTime.Now:yyyyMMddHHmmss}";
        }

        try
        {
            gameMap.Save(fileName);
            Console.WriteLine($"Map saved to {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving map: {ex.Message}");
        }
    }

    private void HandleEditorInput(ConsoleKey key)
    {
        MoveCursor(key);
        PlaceTile(key);
    }

    private void MoveCursor(ConsoleKey key)
    {
        var newPosition = Map.CalculateNewPosition(cursorPosition, key);
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
            ConsoleKey.D8 => MapCodes.Exit,
            _ => null
        };

        if (tile.HasValue)
        {
            gameMap.UpdateMapData(cursorPosition, tile.Value);
            gameMap.PlaceBreadcrumbs();
        }
    }
}
