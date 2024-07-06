namespace Consouler;

internal class Player(string name, (int, int) startingPosition)
{
    public string Name { get; } = name;
    private (int x, int y) Position { get; set; } = startingPosition;

    public (int, int) GetPosition() => Position;

    public void Move(GameMap map, ConsoleKey key)
    {
        var newPosition = GameMap.CalculateNewPosition(Position, key);
        if (map.IsValidPosition(newPosition))
        {
            Position = newPosition;
        }
    }
}