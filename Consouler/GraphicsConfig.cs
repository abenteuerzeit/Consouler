using System.Text;

namespace Consouler;

internal class GraphicsConfig
{
    public char Undefined { get; set; } = ' ';
    public char Path { get; set; } = '░';
    public char Player { get; set; } = '⚔';
    public char Obstacle { get; set; } = '█';
    public char Exit { get; set; } = '·';
    public char Breadcrumb { get; set; } = '•';
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