namespace Consouler;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Consouler");

        bool isRunning = true;

        while (isRunning)
        {
            var game = new Game();
            isRunning = game.Run();
        }
    }
}
