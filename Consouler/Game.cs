namespace Consouler;

internal class Game
{
    private Player player;
    private Map map;
    private bool showBreadcrumbs = false;
    private readonly ConsoleBuffer consoleBuffer;

#pragma warning disable CS8618
    public Game()
#pragma warning restore CS8618
    {
        InitializeGame();
        consoleBuffer = new ConsoleBuffer(Console.WindowWidth, Console.WindowHeight);
        Console.CursorVisible = false;
    }

    private void InitializeGame()
    {
        var mapSize = (width: 31, height: 31);
        var graphicsConfig = new GraphicsConfig
        {
            Player = 'X'
        };
        map = new Map(mapSize, graphicsConfig);
        map.GenerateRandomMap();
        player = new Player(map.GetPlayerPosition());

        Console.WindowWidth = Math.Max(Console.WindowWidth, mapSize.width + 5);
        Console.WindowHeight = Math.Max(Console.WindowHeight, mapSize.height + 5);
    }

    public bool Run()
    {
        while (true)
        {
            if (!PlayGame())
            {
                return false;
            }

            ResetGame();
        }
    }

    private void ResetGame()
    {
        InitializeGame();
    }

    private void RenderFrame()
    {
        consoleBuffer.Clear();
        consoleBuffer.Write(0, 0, "Press E to enter editor mode, G to play game, B to toggle breadcrumbs, Q to quit");
        string mapRendering = map.Render(showBreadcrumbs);
        string[] mapLines = mapRendering.Split('\n');
        for (int i = 0; i < mapLines.Length && i < consoleBuffer.Height - 2; i++)
        {
            consoleBuffer.Write(0, i + 2, mapLines[i]);
        }
        consoleBuffer.Render();
    }

    private void MovePlayer(ConsoleKey key)
    {
        map.UpdateMapData(player.GetPosition(), MapCodes.Undefined);
        player.Move(map, key);
        map.UpdateMapData(player.GetPosition(), MapCodes.Player);
    }

    private void EnterEditorMode()
    {
        var editor = new Editor(map, consoleBuffer);
        editor.Run();
        player = new Player(map.GetPlayerPosition());
    }


    private bool PlayGame()
    {
        while (true)
        {
            RenderFrame();

            if (map.IsExit(player.GetPosition()))
            {
                var message = "You found the exit! Press any key to start a new game or Q to quit...";
                consoleBuffer.Write(0, consoleBuffer.Height - 1, message);
                consoleBuffer.Render();
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Q)
                {
                    return false;
                }
                return true;
            }

            var playerKey = Console.ReadKey(true).Key;

            switch (playerKey)
            {
                case ConsoleKey.Q:
                    return false;
                case ConsoleKey.E:
                    EnterEditorMode();
                    break;
                case ConsoleKey.B:
                    showBreadcrumbs = !showBreadcrumbs;
                    break;
                default:
                    MovePlayer(playerKey);
                    break;
            }
        }
    }
}
