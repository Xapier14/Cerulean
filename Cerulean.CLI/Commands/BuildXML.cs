using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI.Commands;

[CommandName("build-xml")]
[CommandDescription("Builds layouts and styles from CeruleanXMLs.")]
public class BuildXml : ICommand
{
    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
    {
        // file extension to process
        const string fileExtension = ".xml";

        // initialize project vars
        var projectPath = "./";
        var outputPath = Path.Join(projectPath, ".cerulean");
        if (args.Length > 0)
            projectPath = args[0];
        if (args.Length > 1)
            outputPath = args[1];

        // ensure paths exists
        if (!Directory.Exists(projectPath))
        {
            ColoredConsole.WriteLine($"$red^Project path '{projectPath}' does not exist.$r^");
            return -1;
        }

        Directory.CreateDirectory(outputPath);

        // ensure project exists
        if (!Helper.CheckProjectExists(projectPath))
        {
            ColoredConsole.WriteLine("$red^A C# project does not exist in the current directory.$r^");
            return -2;
        }

        // create builder session context
        DirectoryInfo outDirInfo = new(outputPath);
        BuilderContext context = new();

        // check if slient flag is raised
        if (flags.Contains("silent"))
            ColoredConsole.Disable();

        // build XMLs in project directory
        DirectoryInfo dirInfo = new(projectPath);
        var xmlFiles = dirInfo.GetAllFiles()
            .Where(
                fileInfo => fileInfo.Name.ToLower().EndsWith(fileExtension.ToLower())
            )
            .Select(fileInfo => fileInfo.FullName);
        foreach (var file in xmlFiles)
        {
            // reset imports
            var builder = new Builder();
            context.UseDefaultImports();
            ColoredConsole.WriteLine(builder.BuildContextFromXml(context, file)
                ? $"[$green^GOOD$r^][$yellow^XML$r^] '{file}'"
                : $"[$red^FAIL$r^][$yellow^XML$r^] '{file}'");
        }

        var files = outDirInfo.GetAllFiles();
        var dirs = outDirInfo.GetDirectories();

        // delete top-level files
        var cleanErrors = files.Sum(file => file.TryDelete("[$red^FAIL$r^][$cyan^CLEAN$r^]"));

        // delete sub-directories
        cleanErrors += dirs.Sum(dir => dir.TryDelete(true, "[$red^FAIL$r^][$cyan^CLEAN$r^]"));

        ColoredConsole.WriteLine(cleanErrors > 0
            ? $"[$yellow^WARN$r^][$cyan^CLEAN$r^] Directory cleaned with {cleanErrors} errors."
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

        ColoredConsole.Enable();
        return 0;
    }
}