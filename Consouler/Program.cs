namespace Consouler;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Consouler");
        var gameMap = new GameMap(150);
        gameMap.GenerateRandomMap();
        var player = new Player("Adrian", gameMap.GetPlayerInit());

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Press E to enter editor mode, G to play game, Q to quit");
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Q)
            {
                break;
            }
            else if (key == ConsoleKey.E)
            {
                var editor = new Editor(gameMap);
                editor.Run();
            }
            else if (key == ConsoleKey.G)
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine(gameMap.Render());
                    gameMap.UpdateMapData(player.GetPosition(), MapCodes.Undefined);
                    var playerKey = Console.ReadKey().Key;
                    player.Move(gameMap, playerKey);
                    gameMap.UpdateMapData(player.GetPosition(), MapCodes.Player);

                    if (playerKey == ConsoleKey.Q)
                    {
                        break;
                    }
                }
            }
        }
    }

}

