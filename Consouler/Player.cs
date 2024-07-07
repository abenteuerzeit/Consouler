using Consouler;

internal class Player((int, int) startingPosition)
{
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