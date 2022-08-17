using System.Diagnostics;
using System.Text;
using Cerulean.CLI.Attributes;

namespace Cerulean.CLI.Commands;

[CommandName("new")]
[CommandDescription("Generates and scaffolds a new Cerulean UI project.")]
public class NewProject : ICommand
{
    private static void CreateProjectDirectory(string workingDir)
    {
        // confirm if user wants to create a project in working directory
        Console.WriteLine("A project will be created in the folder {0}.", workingDir);
        Console.Write("Do you want to proceed? (Y/n): ");
        if (Console.ReadLine() is { } choice
            && choice.ToLower() == "n")
            return;

        // check if directory already exists and has a project/solution already
        if (Directory.Exists(workingDir)
            && new DirectoryInfo(workingDir)
                .EnumerateFiles()
                .Any(file => file.Extension.ToLower() is ".csproj" or ".sln"))
        {
            Console.WriteLine("Directory already contains a project or solution file.");
            Console.WriteLine("Aborting command...");
        }

        // ensure directory exists
        Directory.CreateDirectory(workingDir);
    }

    private static void CreateProjectBoilerplate(string workingDir)
    {
        Console.WriteLine("Creating project boilerplate + settings...");
        File.WriteAllText(workingDir + "/Usings.cs", USINGS_BOILERPLATE);
        File.WriteAllText(workingDir + "/Program.cs", PROGRAM_BOILERPLATE);
        File.WriteAllText(workingDir + "/ExampleLayout.xml", LAYOUT_BOILERPLATE);
        File.WriteAllText(workingDir + "/.gitignore", GIT_IGNORE_LIST);
        // inject includes item group to project xml
        var projectDirInfo = new DirectoryInfo(workingDir);
        var projectInfo = projectDirInfo
            .EnumerateFiles()
            .First(fileInfo => fileInfo.Extension.ToLower() == ".csproj");
        var projectStream = File.OpenWrite(projectInfo.FullName);
        var seekAmount = "</Project>".Length + 2;
        projectStream.Seek(-seekAmount, SeekOrigin.End);
        var bytesToInject = Encoding.UTF8.GetBytes(PROJECT_XML_INJECT);
        projectStream.Write(bytesToInject);
        projectStream.Close();
        Console.WriteLine("Initialized project!\n");
    }

    private static string DetermineWorkingDirectoryFromArgs(IEnumerable<string> args)
    {
        var workingDir = Environment.CurrentDirectory;
        foreach (var arg in args)
        {
            if (arg.StartsWith('-'))
                continue;
            return arg;
        }

        return Path.GetFullPath(workingDir);
    }

    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
    {
        // get current configuration
        var config = Config.GetConfig();

        // get working directory
        var workingDir = DetermineWorkingDirectoryFromArgs(args);

        // create project dir
        CreateProjectDirectory(workingDir);

        // create dotnet console project
        if (Helper.DoTask("Creating .NET console project...",
                "dotnet",
                "new console", workingDir))
            return -1;

        // create a git repository
        if (Helper.DoTask("Creating a git repository...",
                "git",
                "init", workingDir))
            return -2;

        // add Cerulean as git submodule
        if (Helper.DoTask("Adding CeruleanUI as a submodule...",
                "git",
                "submodule add " + config.GetProperty<string>("CERULEAN_UI_GIT"), workingDir))
            return -3;

        // pull the submodule
        if (Helper.DoTask("Pulling submodule(s)...",
                "git",
                "submodule update --init --recursive", workingDir))
            return -4;

        // add project references
        if (Helper.DoTask("Adding reference to Cerulean.Common.",
                "dotnet",
                "add reference Cerulean/Cerulean.Common/Cerulean.Common.csproj", workingDir))
            return -5;
        if (Helper.DoTask("Adding reference to Cerulean.Core.",
                "dotnet",
                "add reference Cerulean/Cerulean.Core/Cerulean.Core.csproj", workingDir))
            return -6;
        if (Helper.DoTask("Adding reference to Cerulean.Components.",
                "dotnet",
                "add reference Cerulean/Cerulean.Components/Cerulean.Components.csproj", workingDir))
            return -7;

        // initialize project
        CreateProjectBoilerplate(workingDir);

        // commit as initial repo commit
        if (Helper.DoTask("Doing initial commit (1/2)...", "git", "add .", workingDir))
            return -8;
        if (Helper.DoTask("Doing initial commit (2/2)...", "git", "commit -m \"Initial commit via crn\"", workingDir))
            return -9;

        ColoredConsole.WriteLine("$green^Project has been created!$r^");
        Console.WriteLine("Commands to use inside the directory:");
        ColoredConsole.WriteLine("  > $cyan^crn$r^ $yellow^run$r^ - builds XML files and runs the .NET project.");
        ColoredConsole.WriteLine("  > $cyan^crn$r^ $yellow^build$r^ - builds XML files and the .NET project.");
        ColoredConsole.WriteLine(
            "  > $cyan^crn$r^ $yellow^build-xml$r^ - only builds XML files in the project directory.");
        ColoredConsole.WriteLine(
            "  > $cyan^crn$r^ $yellow^publish$r^ - builds a release config of the project and bundles SDL if possible.");

        return 0;
    }

    #region BOILERPLATE CONTENT

    private const string USINGS_BOILERPLATE
        = "global using Cerulean.Common;\n" +
          "global using Cerulean.Core;\n" +
          "global using Cerulean.Components;\n";

    private const string PROGRAM_BOILERPLATE
        = "var ceruleanApi = CeruleanAPI.GetAPI()\n" +
          "                             .UseSDL2Graphics()\n" +
          "                             .UseConsoleLogger()\n" +
          "                             .Initialize();\n" +
          "\n" +
          "var window = ceruleanApi.CreateWindow(\"ExampleLayout\");\n" +
          "\n" +
          "ceruleanApi.WaitForAllWindowsClosed(true);\n";

    private const string LAYOUT_BOILERPLATE
        = "<?xml version='1.0' encoding='UTF-8'?>\n" +
          "<CeruleanXML xmlns:a='Attribute'>\n" +
          "  <Import>Cerulean.Components</Import>\n" +
          "  <Layout Name='ExampleLayout'>\n" +
          "    <Label\n" +
          "      Name='Label_Test'\n" +
          "      ForeColor='#000'\n" +
          "      X='16'\n" +
          "      Y='16'\n" +
          "      Text='Hello World!' />\n" +
          "  </Layout>\n" +
          "</CeruleanXML>\n";

    private const string PROJECT_XML_INJECT
        = "  <ItemGroup>\n" +
          "    <Compile Include=\".cerulean\\*.cs\" />\n" +
          "    <Compile Remove=\"Cerulean\\**\" />\n" +
          "    <Compile Include=\"Layouts\\*.cs\" />\n" +
          "    <Compile Include=\"Styles\\*.cs\" />\n" +
          "  </ItemGroup>\n" +
          "</Project>\n";

    private const string GIT_IGNORE_LIST
        = ".cerulean/\n" +
          ".dependencies/\n" +
          "bin/\n" +
          "obj/\n" +
          "scripts/\n" +
          "publish/\n";

    #endregion
}