using Cerulean.CLI.Extensions;

namespace Cerulean.CLI.Commands
{
    public class BuildXML : ICommand
    {
        public static string? CommandName { get; set; } = "build-xml";

        public static void DoAction(string[] args)
        {
            const string fileExtension = ".xml";

            var projectPath = "./";
            var outputPath = "./.cerulean";
            if (args.Length > 0)
                projectPath = args[0];
            if (args.Length > 1)
                outputPath = args[1];
            if (!Directory.Exists(projectPath))
            {
                ColoredConsole.WriteLine($"$red^Project path '{projectPath}' does not exist.$r^");
                Environment.Exit(-1);
            }
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            var projectPathInfo = new DirectoryInfo(projectPath);
            var projectExists = projectPathInfo
                .EnumerateFiles()
                .Any(fileInfo => fileInfo.Extension.ToLower() == ".csproj");
            if (!projectExists)
            {
                ColoredConsole.WriteLine($"$red^A C# project does not exist in the current directory.$r^");
                Environment.Exit(-2);
            }

            DirectoryInfo dirInfo = new(projectPath);
            DirectoryInfo outDirInfo = new(outputPath);
            BuilderContext context = new();

            // build XMLs in project directory
            foreach (var file in dirInfo.GetAllFiles())
            {
                if (!file.Name.ToLower().EndsWith(fileExtension.ToLower())) continue;
                // reset imports
                context.UseDefaultImports();
                ColoredConsole.WriteLine(Builder.BuildContextFromXML(context, file.FullName)
                    ? $"[$green^GOOD$r^][$yellow^XML$r^] '{file.FullName}'"
                    : $"[$red^FAIL$r^][$yellow^XML$r^] '{file.FullName}'");
            }

            // clean output directory
            var files = outDirInfo.GetAllFiles();
            var dirs = outDirInfo.GetDirectories();
            var cleanErrors = 0;
            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    ColoredConsole.WriteLine($"[$red^FAIL$r^][$cyan^CLEAN$r^] Could not delete file '{file.FullName}'. {ex.Message}");
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
                    ColoredConsole.WriteLine($"[$red^FAIL$r^][$cyan^CLEAN$r^] Could not delete directory '{dir.FullName}'. {ex.Message}");
                    cleanErrors++;
                }
            }
            ColoredConsole.WriteLine(cleanErrors > 0 ? $"[$yellow^WARN$r^][$cyan^CLEAN$r^] Directory cleaned with {cleanErrors} errors."
                                                      : "[$green^GOOD$r^][$cyan^CLEAN$r^] Directory cleaned successfully.");

            // write all exportable files
            var index = 0;
            foreach (var pair in context.Layouts)
            {
                ColoredConsole.WriteLine($"[$cyan^EXPORT$r^] Exporting layout '{pair.Key}'...");
                File.WriteAllText(outDirInfo.FullName + $"/layout{index}.cs", pair.Value);
                index++;
            }
            index = 0;
            foreach (var pair in context.Styles)
            {
                ColoredConsole.WriteLine($"[$cyan^EXPORT$r^] Exporting style '{pair.Key}'...");
                File.WriteAllText(outDirInfo.FullName + $"/style{index}.cs", pair.Value);
                index++;
            }
        }
    }
}
