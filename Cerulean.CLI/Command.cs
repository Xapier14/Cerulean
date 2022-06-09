using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    internal class Command
    {
        public string CommandName { get; init; }
        public Action<string[]> Action { get; init; }
        public Command(string commandName, Action<string[]> action)
        {
            CommandName = commandName;
            Action = action;
        }
        public void DoAction(params string[] args)
        {
            Action(args);
        }
    }
}
