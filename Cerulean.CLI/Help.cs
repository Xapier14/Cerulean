using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cerulean.CLI
{
    internal class Splash
    {
        public static void DisplayGeneralHelp()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"  ______                _                    _____ _     ___ ");
            Console.WriteLine(@" / _____|              | |                  /  ___| |   |_ _|");
            Console.WriteLine(@" | |     ___ _ __ _   _| | ___  __ _ _ ___  | |   | |    | | ");
            Console.WriteLine(@" | |    / _ \ '__| | | | |/ _ \/ _` | '_  \ | |   | |    | | ");
            Console.WriteLine(@" | |____| __/ |  | |_| | |  __/ (_| | | | | | |___| |___ | | ");
            Console.WriteLine(@" \______\___|_|  \___,_|_|\___|\__,_|_| |_| \_____|_____|___|");
            Console.WriteLine();
            Console.ResetColor();
            var name = "crn.exe";
            try
            {
                var version = FileVersionInfo.GetVersionInfo(Path.Combine(AppContext.BaseDirectory, "crn.exe"));
                name = version.InternalName ?? "crn.exe";
                Console.WriteLine($" Cerulean CLI: {version.FileMajorPart}.{version.FileMajorPart}.{version.FileBuildPart}");
            }
            catch
            {
                ColoredConsole.WriteLine(" Cerulean CLI: ?.?.? $red^(malformed executable?)$r^");
            }
            var cli = name.Remove(name.Length - 4, 4);

            var process = new Process
            {
                StartInfo = new()
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
            var versionString = process.StandardOutput.ReadToEnd();
            var match = Regex.Match(versionString, @"(\d+).(\d+).(\d+)");
            var netMajor = int.Parse(match.Groups[1].Value);
            var netMinor = int.Parse(match.Groups[2].Value);
            var netBuild = int.Parse(match.Groups[3].Value);
            var outdated = netMajor < 6;
            ColoredConsole.WriteLine($" .NET SDK: {netMajor}.{netMinor}.{netBuild} " +
                                     (outdated ? "$red^(outdated)$r^" : "$green^(supported)$r^"));
            Console.WriteLine();

            Console.WriteLine("Usage:");
            ColoredConsole.WriteLine($"\t$cyan^{cli}$r^ $yellow^<command> [args]$r^");
            Console.WriteLine("Example:");
            ColoredConsole.WriteLine($"\t$cyan^{cli}$r^ $yellow^help build-xml$r^");
        }
    }
}
