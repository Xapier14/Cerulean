using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cerulean.CLI.Attributes;

namespace Cerulean.CLI.Commands
{
    [CommandName("build")]
    [CommandAlias("compile", "b")]
    [CommandDescription("Build a debug configuration of the cerulean project.")]
    public class BuildProject : ICommand
    {
        private static bool Build(string projectPath, string arch, string os, string config)
        {
            var targetRuntime = $"{os}-{arch}";
            if (!Helper.DoTask(null,
                    "dotnet",
                    $"build -c {config} -r {targetRuntime} --no-self-contained",
                    projectPath,
                    false))
                return false;

            ColoredConsole.WriteLine("$red^Error building project file.$r^");

            return true;
        }

        public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
        {
            var projectPath = "./";
            if (args.Length > 0)
                projectPath = args[0];

            // error if project file does not exist.
            if (!Helper.CheckProjectExists(projectPath))
            {
                return -1;
            }

            var config = Config.GetConfig();

            // determine target runtime
            options.TryGetValue("arch", out var arch);
            arch ??= Environment.Is64BitOperatingSystem ? "x64" : "x86";

            options.TryGetValue("os", out var os);
            os ??= Helper.GetOSPlatform();

            options.TryGetValue("config", out var netConfig);
            netConfig ??= config.GetProperty<string>("DOTNET_DEFAULT_BUILD_CONFIG") ?? "Debug";

            if (os is null)
            {
                ColoredConsole.WriteLine("red^[Error]$r^ Operating system is unsupported.");
                return -2;
            }
            options.TryGetValue("runtime", out var runtime);
            runtime ??= $"{os}-{arch}";
            ColoredConsole.WriteLine("$yellow^[TARGET]$r^ Target runtime is " + runtime + ".");

            // Build the XMLs
            ColoredConsole.WriteLine("$yellow^[CRN]$r^ Building XMLs...");
            Router.GetRouter().ExecuteCommand("build-xml", projectPath);

            // Build dotnet project
            ColoredConsole.WriteLine("$yellow^[DOTNET]$r^ Building project...");
            if (Build(projectPath, arch, os, netConfig))
                return -3;

            // get csproj info
            var csproj = Helper.GetProjectFileInDirectory(projectPath);
            var netVersion = Helper.GetXMLNetVersion(csproj);

            // Bundle dependencies if not found
            ColoredConsole.WriteLine("$yellow^[CRN]$r^ Assessing dependencies...");
            Router.GetRouter().ExecuteCommand("bundle", 
                projectPath, 
                "-arch", arch, 
                "-os", os,
                "-config", netConfig,
                "-nv", netVersion);

            Console.WriteLine();
            ColoredConsole.WriteLine("$green^Cerulean project built successfully!$r^");
            ColoredConsole.WriteLine("Try running with '$yellow^crn run$r^'");

            return 0;
        }
    }
}
