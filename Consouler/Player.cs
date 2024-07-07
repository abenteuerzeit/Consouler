namespace Consouler;

internal class Player(string name, (int, int) startingPosition)
{
    public string Name { get; } = name;
    private (int x, int y) Position { get; set; } = startingPosition;

    public (int, int) GetPosition() => Position;

    public void Move(Map map, ConsoleKey key)
    {
        var newPosition = Map.CalculateNewPosition(Position, key);
        if (map.IsValidPosition(newPosition))
        {
            Position = newPosition;
        }
    }
}