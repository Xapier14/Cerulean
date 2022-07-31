using System.Text;

namespace Cerulean.CLI;

public static class ColoredConsole
{
    private static readonly object _lock = new();

    private static void ChangeColor(string color)
    {
        if (color.ToLower() is "reset" or "r")
        {
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = color.ToLower() switch
        {
            "black" => ConsoleColor.Black,
            "blue" => ConsoleColor.Blue,
            "cyan" => ConsoleColor.Cyan,
            "darkblue" => ConsoleColor.DarkBlue,
            "darkcyan" => ConsoleColor.DarkCyan,
            "darkgray" => ConsoleColor.DarkGray,
            "darkgreen" => ConsoleColor.DarkGreen,
            "darkmagenta" => ConsoleColor.DarkMagenta,
            "darkred" => ConsoleColor.DarkRed,
            "darkyellow" => ConsoleColor.DarkYellow,
            "gray" => ConsoleColor.Gray,
            "green" => ConsoleColor.Green,
            "magenta" => ConsoleColor.Magenta,
            "red" => ConsoleColor.Red,
            "white" => ConsoleColor.White,
            "yellow" => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };
    }

    public static void Write(string message)
    {
        lock (_lock)
        {
            StringBuilder buffer = new();
            foreach (var c in message)
                if (buffer.Length > 0)
                {
                    if (c != '^')
                    {
                        buffer.Append(c);
                        continue;
                    }

                    // change color
                    var colorStr = buffer.ToString()[1..];
                    buffer.Clear();
                    ChangeColor(colorStr);
                }
                else
                {
                    if (c == '$')
                    {
                        buffer.Append(c);
                        continue;
                    }

                    Console.Write(c);
                }
        }
    }

    public static void WriteLine(string message)
    {
        Write(message + "\n");
    }
}