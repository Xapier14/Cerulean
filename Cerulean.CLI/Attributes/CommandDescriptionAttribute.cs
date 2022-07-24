using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandDescriptionAttribute : Attribute
    {
        public string CommandDescription { get; init; }

        public CommandDescriptionAttribute(string commandDescription)
        {
            CommandDescription = commandDescription;
        }
    }
}
