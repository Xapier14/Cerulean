using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cerulean.CLI
{
    internal class Help
    {
        public static void DisplayGeneralHelp()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"  ______                _                    _____ _     ___ ");
            Console.WriteLine(@" / _____|              | |                  /  ___| |   |_ _|");
            Console.WriteLine(@" | |     ___ _ __ _   _| | ___  __ _ _ ___  | |   | |    | |  ");
            Console.WriteLine(@" | |    / _ \ '__| | | | |/ _ \/ _` | '_  \ | |   | |    | |  ");
            Console.WriteLine(@" | |____| __/ |  | |_| | |  __/ (_| | | | | | |___| |___ | | ");
            Console.WriteLine(@" \______\___|_|  \___,_|_|\___|\__,_|_| |_| \_____|_____|___|");
            Console.WriteLine();
            Console.ResetColor();
            string name = "crn.exe";
            try
            {
                var version = FileVersionInfo.GetVersionInfo(Path.Combine(AppContext.BaseDirectory, "crn.exe"));
                name = version.InternalName ?? "crn.exe";
                Console.WriteLine(" Cerulean CLI: {0}.{1}.{2}", version.FileMajorPart, version.FileMinorPart, version.FileBuildPart);
            }
            catch
            {
                Console.WriteLine(" Cerulean CLI: ?.?.? (malformed executable?)");
            }
            var cli = name.Remove(name.Length-4, 4);

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
            Match match = Regex.Match(versionString, @"(\d+).(\d+).(\d+)");
            var netMajor = int.Parse(match.Groups[1].Value);
            var netMinor = int.Parse(match.Groups[2].Value);
            var netBuild = int.Parse(match.Groups[3].Value);
            Console.WriteLine(".net SDK: {0}.{1}.{2}", netMajor, netMinor, netBuild);
            Console.WriteLine();

            Console.WriteLine("Usage:");
            Console.WriteLine($"\t{cli} <command> [args]");
            Console.WriteLine("Example:");
            Console.WriteLine($"\t{cli} help xml");
        }
    }
}
