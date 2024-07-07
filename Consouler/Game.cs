namespace Consouler;

internal class Game
{
    private readonly Player player;
    private readonly Map map;
    private bool showBreadcrumbs = false;

    public Game()
    {
        var mapSize = (height: 30, width: 30);
        var graphicsConfig = new GraphicsConfig
        {
            Player = 'X',
            Friend = ' ', // TODO: implement gameplay logic
            Enemy = ' '
        };
        map = new Map(mapSize, graphicsConfig);
        map.GenerateRandomMap();
        player = new Player("Adrian", map.GetPlayerPosition());
    }

    public bool Run()
    {
        bool isRunning = true;

        while (isRunning)
        {
            Console.Clear();
            Console.WriteLine("Press E to enter editor mode, G to play game, B to toggle breadcrumbs, Q to quit");
            Console.WriteLine(map.Render(showBreadcrumbs));
            map.UpdateMapData(player.GetPosition(), MapCodes.Undefined);

            if (map.IsExit(player.GetPosition()))
            {
                Console.WriteLine("You found the exit!");
                return true; // return to menu
            }

            var key = Console.ReadKey().Key;

            switch (key)
            {
                case ConsoleKey.Q:
                    return false;
                case ConsoleKey.E:
                    EnterEditorMode();
                    break;
                case ConsoleKey.G:
                    PlayGame();
                    break;
                case ConsoleKey.B:
                    showBreadcrumbs = !showBreadcrumbs;
                    break;
                default:
                    player.Move(map, key);
                    break;
            }

            map.UpdateMapData(player.GetPosition(), MapCodes.Player);

        }

        return isRunning;
    }

    private void EnterEditorMode()
    {
        var editor = new Editor(map);
        editor.Run();
    }

    private void PlayGame()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Press E to enter editor mode, G to play game, Q to quit");
            Console.WriteLine(map.Render(showBreadcrumbs));
            map.UpdateMapData(player.GetPosition(), MapCodes.Undefined);

            if (map.IsExit(player.GetPosition()))
            {
                Console.WriteLine("You found the exit!");
                break;
            }

            var playerKey = Console.ReadKey().Key;

            if (playerKey == ConsoleKey.Q)
            {
                break;
            }

            player.Move(map, playerKey);
            map.UpdateMapData(player.GetPosition(), MapCodes.Player);
        }
    }
}
