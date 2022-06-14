using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Commands
{
    public class NewProject : ICommand
    {
        public static string? CommandName { get; set; } = "new";
        public static void DoAction(string[] args)
        {

        }
    }
}
