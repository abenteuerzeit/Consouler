namespace Consouler;

class Editor
{
    private (int x, int y) cursorPosition = (0, 0);
    private readonly Map map;
    private readonly ConsoleBuffer consoleBuffer;

    public Editor(Map map, ConsoleBuffer buffer)
    {
        this.map = map;
        consoleBuffer = buffer;
        Console.CursorVisible = true;
    }

    public void Run()
    {
        while (true)
        {
            RenderEditor();
            Console.SetCursorPosition(cursorPosition.y + 1, cursorPosition.x + 1);

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Q)
            {
                Console.CursorVisible = false;
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

    private void RenderEditor()
    {
        consoleBuffer.Clear();
        string renderedMap = map.Render();
        string[] mapLines = renderedMap.Split('\n');

        for (int i = 0; i < mapLines.Length; i++)
        {
            consoleBuffer.Write(0, i, mapLines[i]);
        }

        DisplayEditorMenu();
        consoleBuffer.Render();
    }

    private void DisplayEditorMenu()
    {
        string[] menuLines = {
            "Editor Mode: Use arrow keys to move, 1-5 to place tiles, S to save, Q to quit",
            "1 => Player",
            "2 => Path",
            "3 => Obstacle",
            "4 => Exit",
            "5 => Undefined"
        };

        var mapSize = map.GetSize();

        for (int i = 0; i < menuLines.Length; i++)
        {
            consoleBuffer.Write(0, mapSize.height + i + 1, menuLines[i]);
        }
    }

    private void SaveMap()
    {
        var mapSize = map.GetSize();

        Console.SetCursorPosition(0, mapSize.height + 8);
        Console.Write("Enter filename to save: ");
        var fileName = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"map-{DateTime.Now:yyyyMMddHHmmss}";
        }

        try
        {
            map.Save(fileName);
            Console.WriteLine($"Map saved to {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving map: {ex.Message}");
        }

        Console.SetCursorPosition(cursorPosition.y + 1, cursorPosition.x + 1);
    }

    private void HandleEditorInput(ConsoleKey key)
    {
        MoveCursor(key);
        PlaceTile(key);
    }

    private void MoveCursor(ConsoleKey key)
    {
        var newPosition = Map.CalculateNewPosition(cursorPosition, key);
        var mapSize = map.GetSize();

        if (newPosition.x >= 0 && newPosition.y >= 0 && newPosition.x < mapSize.height && newPosition.y < mapSize.width)
        {
            cursorPosition = newPosition;
        }
    }

    private void PlaceTile(ConsoleKey key)
    {
        MapCodes? tile = key switch
        {
            ConsoleKey.D1 => MapCodes.Player,
            ConsoleKey.D2 => MapCodes.Path,
            ConsoleKey.D3 => MapCodes.Obstacle,
            ConsoleKey.D4 => MapCodes.Exit,
            ConsoleKey.D5 => MapCodes.Undefined,
            _ => null
        };

        if (tile.HasValue)
        {
            map.UpdateMapData(cursorPosition, tile.Value);
        }
    }
}
