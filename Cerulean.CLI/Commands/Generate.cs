using Cerulean.CLI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Commands
{
    [CommandName("generate")]
    [CommandAlias("g", "gen")]
    [CommandDescription("Generates and scaffolds Cerulean UI resource.")]
    internal class Generate : ICommand
    {
        public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
        {
            throw new NotImplementedException();
        }
    }
}
