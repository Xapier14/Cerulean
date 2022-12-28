using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.CLI.Attributes;

namespace Cerulean.CLI.Commands
{
    [CommandName("run")]
    [CommandAlias("r")]
    [CommandDescription("Builds XMLs and runs the cerulean project.")]
    internal class RunProject : ICommand
    {
        public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
        {
            var projectPath = "./";
            if (args.Length > 0)
                projectPath = args[0];

            var router = Router.GetRouter();
            router.ExecuteCommand("build-xml", "-silent");
            
            var config = Config.GetConfig();

            options.TryGetValue("arch", out var arch);
            arch ??= Environment.Is64BitOperatingSystem ? "x64" : "x86";

            options.TryGetValue("os", out var os);
            os ??= Helper.GetOSPlatform();

            options.TryGetValue("config", out var netConfig);
            netConfig ??= config.GetProperty<string>("DOTNET_DEFAULT_BUILD_CONFIG");

            var runtime = $"{os}-{arch}";

            if (Helper.DoTask("Running project...",
                    "dotnet",
                    $"run -r {runtime} -c {netConfig} --self-contained=false",
                    projectPath,
                    false))
                return -1;
            return 0;
        }
    }
}
