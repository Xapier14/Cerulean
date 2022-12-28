using Cerulean.CLI.Attributes;
using Cerulean.Common;

namespace Cerulean.CLI.Commands;

[CommandName("help")]
[CommandAlias("h", "?")]
[CommandDescription("Displays help information for all or specific commands.")]
public class Help : ICommand
{
    private static void PrintCommandInfo(string command, string description)
    {
        var consoleWidth = Console.BufferWidth - 4;
        var commandWidth = (int)Math.Floor(0.15 * consoleWidth);
        var descriptionWidth = (int)Math.Floor(0.85 * consoleWidth);

        var commandString = Helper.FormatString(command, commandWidth);
        var descriptionStrings = Helper.WordWrap(description, descriptionWidth)
            .Select(line => Helper.FormatString(line, descriptionWidth)).ToArray();

        ColoredConsole.WriteLine($" $cyan^{commandString}$r^ - {descriptionStrings[0]}");
        for (var i = 1; i < descriptionStrings.Length; i++)
            ColoredConsole.WriteLine($" {Helper.FormatString(string.Empty, commandWidth)} - {descriptionStrings[i]}");
    }

    private static void PrintAllCommandInfo()
    {
        Console.WriteLine("Available commands:");
        var commands = Helper.GetAllCommandInfo();
        foreach (var (command, description) in commands)
            PrintCommandInfo(command, description);
    }

    private static void PrintAvailableComponentRefs()
    {
        Console.WriteLine("Registered Component References: {0}", Helper.CountInterfaceImplementations(typeof(IComponentRef)));
    }

    private static void PrintAvailableElementHandlers()
    {
        Console.WriteLine("Registered Element Handlers: {0}", Helper.CountInterfaceImplementations(typeof(IElementHandler)));
    }

    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
    {
        if (args.Length != 0)
            throw new NotImplementedException();
        Splash.DisplaySplash();
        PrintAllCommandInfo();
        Console.WriteLine();
        PrintAvailableComponentRefs();
        PrintAvailableElementHandlers();
        return 0;

    }
}