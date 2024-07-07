namespace Consouler;

internal class ConsoleBuffer
{
    private readonly char[,] buffer;
    private readonly char[,] previousBuffer;
    public int Width { get; }
    public int Height { get; }

    public ConsoleBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        buffer = new char[height, width];
        previousBuffer = new char[height, width];
        Clear();
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                buffer[y, x] = ' ';
            }
        }
    }

    public void Write(int x, int y, char c)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            buffer[y, x] = c;
        }
    }

    public void Write(int x, int y, string s)
    {
        for (int i = 0; i < s.Length && x + i < Width; i++)
        {
            Write(x + i, y, s[i]);
        }
    }

    public void Render()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (buffer[y, x] != previousBuffer[y, x])
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(buffer[y, x]);
                    previousBuffer[y, x] = buffer[y, x];
                }
            }
        }
    }
}