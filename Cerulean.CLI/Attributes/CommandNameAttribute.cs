using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNameAttribute : Attribute
    {
        public string CommandName { get; init; }

        public CommandNameAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
