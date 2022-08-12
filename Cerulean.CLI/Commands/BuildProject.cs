using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cerulean.CLI.Attributes;

namespace Cerulean.CLI.Commands
{
    [CommandName("build")]
    [CommandDescription("Build a debug configuration of the cerulean project.")]
    public class BuildProject : ICommand
    {
        private static bool Build(string projectPath, string arch, string os, string config = "Debug")
        {
            var targetRuntime = $"{os}-{arch}";
            if (!Helper.DoTask(null,
                    "dotnet",
                    $"build -c {config} -r {targetRuntime}",
                    projectPath))
                return false;

            ColoredConsole.WriteLine("$red^Error building project file.$r^");
            return true;
        }

        public int DoAction(string[] args)
        {
            var projectPath = "./";
            if (args.Length > 0)
                projectPath = args[0];

            // error if project file does not exist.
            if (!Helper.CheckProjectExists(projectPath))
            {
                return -1;
            }

            // determine target runtime
            var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            var os = Helper.GetOSPlatform();
            if (os is null)
            {
                ColoredConsole.WriteLine("red^[Error]$r^ Operating system is unsupported.");
                return -2;
            }
            var runtime = $"{os}-{arch}";
            ColoredConsole.WriteLine("yellow^[TARGET]$r^ Target runtime is " + runtime + ".");

            // Build the XMLs
            ColoredConsole.WriteLine("$yellow^[CRN]$r^ Building XMLs...");
            Router.GetRouter().ExecuteCommand("build-xml", projectPath);

            // Build dotnet project
            ColoredConsole.WriteLine("$yellow^[DOTNET]$r Building project...");
            if (Build(projectPath, arch, os))
                return -3;

            // Bundle dependencies if not found
            ColoredConsole.WriteLine("$yellow^[CRN]$r^ Assessing dependencies...");
            Router.GetRouter().ExecuteCommand("bundle", "");
            return 0;
        }
    }
}
