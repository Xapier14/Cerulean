﻿using Cerulean.CLI.Attributes;

namespace Cerulean.CLI.Commands;

[CommandName("help")]
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
        var commands = Helper.GetAllCommandInfo();
        foreach (var (command, description) in commands) PrintCommandInfo(command, description);
    }

    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
    {
        if (args.Length != 0)
            throw new NotImplementedException();
        Splash.DisplaySplash();
        PrintAllCommandInfo();
        return 0;

    }
}