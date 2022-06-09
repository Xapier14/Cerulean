using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Commands
{
    internal class BuildXML
    {
        public static void Action(string[] args)
        {
            const string fileExtension = ".xml";

            string projectPath = "./";
            string outputPath = "./.cerulean";
            if (args.Length > 0)
                projectPath = args[0];
            if (args.Length > 1)
                outputPath = args[1];
            if (!Directory.Exists(projectPath))
            {
                Console.WriteLine($"Project path '{projectPath}' does not exist.");
                Environment.Exit(-1);
            }
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            DirectoryInfo dirInfo = new(projectPath);
            DirectoryInfo outDirInfo = new(outputPath);
            BuilderContext context = new();

            // build XMLs in project directory
            foreach (FileInfo file in dirInfo.GetAllFiles())
            {
                if (file.Name.ToLower().EndsWith(fileExtension.ToLower()))
                {
                    // reset imports
                    context.UseDefaultImports();
                    if (Builder.BuildContextFromXML(context, file.FullName))
                    {
                        Console.WriteLine("[GOOD][XML] '{0}'", file.FullName);
                    }
                    else
                    {
                        Console.WriteLine("[FAIL][XML] '{0}'", file.FullName);
                    }
                }
            }

            // clean output directory
            var files = outDirInfo.GetAllFiles();
            var dirs = outDirInfo.GetDirectories();
            int cleanErrors = 0;
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[FAIL][CLEAN] Could not delete file '{0}'. {1}", file.FullName, ex.Message);
                    cleanErrors++;
                }
            }
            foreach (var dir in dirs)
            {
                try
                {
                    dir.Delete(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[FAIL][CLEAN] Could not delete directory '{0}'. {1}", dir.FullName, ex.Message);
                    cleanErrors++;
                }
            }
            Console.WriteLine(cleanErrors > 0 ? "[WARN][CLEAN] Directory cleaned with {0} errors."
                                              : "[GOOD][CLEAN] Directory cleaned successfully."
                                              , cleanErrors);

            // write all exportable files
            int index = 0;
            foreach (var pair in context.Layouts)
            {
                Console.WriteLine("[EXPORT] Exporting layout '{0}'...", pair.Key);
                File.WriteAllText(outDirInfo.FullName + $"/layout{index}.cs", pair.Value);
                index++;
            }
            index = 0;
            foreach (var pair in context.Styles)
            {
                Console.WriteLine("[EXPORT] Exporting style '{0}'...", pair.Key);
                File.WriteAllText(outDirInfo.FullName + $"/style{index}.cs", pair.Value);
                index++;
            }
        }
    }
}
