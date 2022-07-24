﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.CLI.Attributes;
using Cerulean.Common;

namespace Cerulean.CLI.Commands
{
    [CommandName("help")]
    [CommandDescription("Displays help information for all or specific commands.")]
    public class Help : ICommand
    {
        public static string? CommandName { get; set; } = "help";

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
            {
                ColoredConsole.WriteLine($" {Helper.FormatString(string.Empty, commandWidth)} - {descriptionStrings[i]}");
            }
        }

        private static void PrintAllCommandInfo()
        {
            var commands = Helper.GetAllCommandInfo();
            foreach (var (command, description) in commands)
            {
                PrintCommandInfo(command, description);
            }
        }

        public static int DoAction(string[] args)
        {
            if (args.Length == 0)
            {
                Splash.DisplaySplash();
                PrintAllCommandInfo();
                return 0;
            }
            throw new NotImplementedException();
        }
    }
}
