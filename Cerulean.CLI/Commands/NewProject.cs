using Cerulean.CLI.Attributes;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;

namespace Cerulean.CLI;

[CommandName("new")]
[CommandAlias("create", "start", "n")]
[CommandDescription("Generates and scaffolds a new Cerulean UI project.")]
public class NewProject : ICommand
{
    private static void CreateProjectDirectory(string workingDir)
    {
        // check if directory already exists and has a project/solution already
        if (Directory.Exists(workingDir)
            && new DirectoryInfo(workingDir)
                .EnumerateFiles()
                .Any(file => file.Extension.ToLower() is ".csproj" or ".sln"))
        {
            Console.WriteLine("Directory already contains a project or solution file.");
            Console.WriteLine("Aborting command...");
            Environment.Exit(0);
        }

        // ensure directory exists
        Directory.CreateDirectory(workingDir);
    }

    private static void CreateProjectBoilerplate(string workingDir)
    {
        Console.WriteLine("Creating project boilerplate + settings...");
        Directory.CreateDirectory(workingDir + "/Layouts");
        Directory.CreateDirectory(workingDir + "/.modules");
        File.WriteAllText(workingDir + "/Usings.cs", USINGS_BOILERPLATE);
        File.WriteAllText(workingDir + "/Program.cs", PROGRAM_BOILERPLATE);
        File.WriteAllText(workingDir + "/Layouts/ExampleLayout.xml", LAYOUT_BOILERPLATE);
        File.WriteAllText(workingDir + "/.gitignore", GIT_IGNORE_LIST);
        File.WriteAllText(workingDir + "/app.manifest", APP_MANIFEST);
        File.WriteAllText(workingDir + "/.modules/Cerulean.Components.xml", CERULEAN_COMPONENTS_TEMP_XML);

        // inject includes item group to project xml
        var projectDirInfo = new DirectoryInfo(workingDir);
        var projectInfo = projectDirInfo
            .EnumerateFiles()
            .First(fileInfo => string.Equals(fileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase));

        var projectXml = XDocument.Load(projectInfo.FullName);
        var root = projectXml.Root;

        var propertyGroup = root?.Element("PropertyGroup");
        var itemGroup = root?.Element("ItemGroup");

        propertyGroup?.Add(new XElement("ApplicationManifest")
        {
            Value = "app.manifest"
        });
        var compile1 = new XElement("Compile");
        compile1.SetAttributeValue("Include", ".cerulean\\*.cs");
        var compile2 = new XElement("Compile");
        compile2.SetAttributeValue("Remove", "Cerulean\\**");
        itemGroup?.Add(compile1);
        itemGroup?.Add(compile2);
        root?.Save(projectInfo.FullName);

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

    public int GenerateBleedingEdge(string workingDir, Config config)
    {
        // create dotnet console project
        if (Helper.DoTask("Creating .NET console project...",
                "dotnet",
                "new console", workingDir))
        {
            return -1;
        }

        // create a git repository
        if (Helper.DoTask("Creating a git repository...",
                "git",
                "init", workingDir))
        {
            return -2;
        }

        // add Cerulean as git submodule
        if (Helper.DoTask("Adding CeruleanUI as a submodule...",
                "git",
                "submodule add " + config.GetProperty<string>("CERULEAN_UI_GIT"), workingDir))
        {
            return -3;
        }

        // pull the submodule
        if (Helper.DoTask("Pulling submodule(s)...",
                "git",
                "submodule update --init --recursive", workingDir))
        {
            return -4;
        }

        // add project references
        if (Helper.DoTask("Adding reference to Cerulean.Common...",
                "dotnet",
                "add reference Cerulean/Cerulean.Common/Cerulean.Common.csproj", workingDir))
        {
            return -5;
        }

        if (Helper.DoTask("Adding reference to Cerulean.Core...",
                "dotnet",
                "add reference Cerulean/Cerulean.Core/Cerulean.Core.csproj", workingDir))
        {
            return -6;
        }

        if (Helper.DoTask("Adding reference to Cerulean.Components...",
                "dotnet",
                "add reference Cerulean/Cerulean.Components/Cerulean.Components.csproj", workingDir))
        {
            return -7;
        }

        return 0;
    }

    public int GenerateStable(string workingDir, Config config)
    {
        // create dotnet console project
        if (Helper.DoTask("Creating .NET console project...",
                "dotnet",
                "new console", workingDir))
        {
            return -1;
        }

        // create a git repository
        if (Helper.DoTask("Creating a git repository...",
                "git",
                "init", workingDir))
        {
            return -2;
        }

        // add Cerulean.Core NuGet package
        if (Helper.DoTask("Adding Cerulean.Core NuGet package...",
                "dotnet",
                "add package Cerulean.Core", workingDir))
        {
            return -3;
        }

        // add Cerulean.Components NuGet package
        if (Helper.DoTask("Adding Cerulean.Components NuGet package...",
                "dotnet",
                "add package Cerulean.Components", workingDir))
        {
            return -4;
        }

        // add Cerulean.SDL2-CS NuGet package
        if (Helper.DoTask("Adding Cerulean.SDL2-CS NuGet package...",
                "dotnet",
                "add package Cerulean.SDL2-CS", workingDir))
        {
            return -5;
        }

        return 0;
    }

    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
    {
        // get current configuration
        var config = Config.GetConfig();

        // get working directory
        var workingDir = DetermineWorkingDirectoryFromArgs(args);

        // confirm if user wants to create a project in working directory
        Console.WriteLine("A project will be created in the folder {0}.", workingDir);
        Console.Write("Do you want to proceed? (Y/n): ");
        if (Console.ReadLine() is { } choice
            && string.Equals(choice, "n", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var template = args.Length > 1 ? args[1] : "stable";

        // create project dir
        CreateProjectDirectory(workingDir);

        var genErrorCode = template.ToLower() switch
        {
            "stable" => GenerateStable(workingDir, config),
            "bleedingedge" => GenerateBleedingEdge(workingDir, config),
            _ => 1
        };

        if (genErrorCode == 1)
        {
            ColoredConsole.WriteLine("$red^Project template is invalid!$rs^");
            return genErrorCode;
        }

        if (genErrorCode != 0)
        {
            ColoredConsole.WriteLine("$red^Error generating project template!$rs^");
            return genErrorCode;
        }

        // initialize project
        CreateProjectBoilerplate(workingDir);

        // commit as initial repo commit
        if (Helper.DoTask("Doing initial commit (1/2)...",
                "git",
                "add .", workingDir))
        {
            return -9;
        }

        if (Helper.DoTask("Doing initial commit (2/2)...",
                "git",
                "commit -m \"Initial commit via crn\"", workingDir))
        {
            return -10;
        }

        var dirInfo = new DirectoryInfo(workingDir);

        ColoredConsole.WriteLine("$green^Project has been created!$r^");
        ColoredConsole.WriteLine($"Change into the directory using: $cyan^cd$r^ $yellow^{dirInfo.Name}$r^");
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
        = "void Callback(CeruleanAPI ceruleanApi)\n" +
          "{\n" +
          "    _ = ceruleanApi.CreateWindow(\"ExampleLayout\");\n" +
          "}\n" +
          "\n" +
          "var ceruleanApi = CeruleanAPI.GetAPI()\n" +
          "                             .UseSDL2Graphics()\n" +
          "                             .UseConsoleLogger()\n" +
          "                             .Initialize(Callback, quitIfNoWindowsOpen: true);\n" +
          "\n";

    private const string LAYOUT_BOILERPLATE
        = "<?xml version='1.0' encoding='UTF-8'?>\n" +
          "<CeruleanXML xmlns:a='Attribute'>\n" +
          "  <Import>Cerulean.Components</Import>\n" +
          "  <Layout Name='ExampleLayout'>\n" +
          "    <Label Name='Label_Test' ForeColor='#000' X='16' Y='16' Text='Hello World!' />\n" +
          "  </Layout>\n" +
          "</CeruleanXML>\n";

    private const string APP_MANIFEST
        = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
          "<assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\">\n" +
          "  <trustInfo xmlns=\"urn:schemas-microsoft-com:asm.v2\">\n" +
          "    <security>\n" +
          "      <requestedPrivileges xmlns=\"urn:schemas-microsoft-com:asm.v3\">\n" +
          "        <requestedExecutionLevel level=\"asInvoker\" uiAccess=\"false\" />\n" +
          "      </requestedPrivileges>\n" +
          "    </security>\n" +
          "  </trustInfo>\n" +
          "  <application xmlns=\"urn:schemas-microsoft-com:asm.v3\">\n" +
          "    <windowsSettings>\n" +
          "      <dpiAware xmlns=\"http://schemas.microsoft.com/SMI/2005/WindowsSettings\">true</dpiAware>\n" +
          "      <longPathAware xmlns=\"http://schemas.microsoft.com/SMI/2016/WindowsSettings\">true</longPathAware>\n" +
          "    </windowsSettings>\n" +
          "  </application>\n" +
          "</assembly>\n";

    private const string GIT_IGNORE_LIST
        = ".cerulean/\n" +
          ".dependencies/\n" +
          "bin/\n" +
          "obj/\n" +
          "scripts/\n" +
          "publish/\n";

    private const string CERULEAN_COMPONENTS_TEMP_XML
        =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<RefXML>\n" +
        "  <Component Name=\"Grid\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"ColumnCount\" Type=\"int\" />\n" +
        "    <Property Name=\"RowCount\" Type=\"int\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Panel\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BorderColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Pointer\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"X\" Type=\"int\" />\n" +
        "    <Property Name=\"Y\" Type=\"int\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Timer\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"IsRunning\" Type=\"bool\" />\n" +
        "    <Property Name=\"Interval\" Type=\"int\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Image\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"ImageSource\" Type=\"string\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BorderColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"PictureMode\" Type=\"Cerulean.Common.PictureMode\" />\n" +
        "    <Property Name=\"Opacity\" Type=\"double\" />\n" +
        "    <Property Name=\"Visible\" Type=\"bool\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Label\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"ForeColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"Text\" Type=\"string\" />\n" +
        "    <Property Name=\"FontName\" Type=\"string\" />\n" +
        "    <Property Name=\"FontSize\" Type=\"int\" />\n" +
        "    <Property Name=\"FontStyle\" Type=\"string\" />\n" +
        "    <Property Name=\"WrapText\" Type=\"bool\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"ProgressBar\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"ForeColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BorderColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"Value\" Type=\"int\" />\n" +
        "    <Property Name=\"Maximum\" Type=\"int\" />\n" +
        "    <Property Name=\"Orientation\" Type=\"Cerulean.Common.Orientation\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Rectangle\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"FillColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BorderColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"FillOpacity\" Type=\"double\" />\n" +
        "    <Property Name=\"BorderOpacity\" Type=\"double\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"Button\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"Text\" Type=\"string\" />\n" +
        "    <Property Name=\"FontName\" Type=\"string\" />\n" +
        "    <Property Name=\"FontSize\" Type=\"int\" />\n" +
        "    <Property Name=\"FontStyle\" Type=\"string\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"ForeColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BorderColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"HighlightColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"ActivatedColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"CheckBox\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"ForeColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"Checked\" Type=\"bool\" />\n" +
        "    <Property Name=\"Text\" Type=\"string\" />\n" +
        "    <Property Name=\"FontName\" Type=\"string\" />\n" +
        "    <Property Name=\"FontSize\" Type=\"int\" />\n" +
        "    <Property Name=\"FontStyle\" Type=\"string\" />\n" +
        "    <Property Name=\"WrapText\" Type=\"bool\" />\n" +
        "    <Property Name=\"InputData\" Type=\"string\" />\n" +
        "    <Property Name=\"InputGroup\" Type=\"string\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"InputContext\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"SubmitButton\" Type=\"component&lt;Cerulean.Component.Button&gt;*\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"RadioButton\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"ForeColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"SelectedColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"Selected\" Type=\"bool\" />\n" +
        "    <Property Name=\"Text\" Type=\"string\" />\n" +
        "    <Property Name=\"FontName\" Type=\"string\" />\n" +
        "    <Property Name=\"FontSize\" Type=\"int\" />\n" +
        "    <Property Name=\"FontStyle\" Type=\"string\" />\n" +
        "    <Property Name=\"WrapText\" Type=\"bool\" />\n" +
        "    <Property Name=\"InputData\" Type=\"string\" />\n" +
        "    <Property Name=\"InputGroup\" Type=\"string\" />\n" +
        "  </Component>\n" +
        "  <Component Name=\"TextBox\" Namespace=\"Cerulean.Components\">\n" +
        "    <Property Name=\"Size\" Type=\"Cerulean.Common.Size\" />\n" +
        "    <Property Name=\"HintW\" Type=\"int\" />\n" +
        "    <Property Name=\"HintH\" Type=\"int\" />\n" +
        "    <Property Name=\"BackColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"BorderColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"FocusedColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"ForeColor\" Type=\"Cerulean.Common.Color\" />\n" +
        "    <Property Name=\"MaxLength\" Type=\"int\" />\n" +
        "    <Property Name=\"Text\" Type=\"string\" />\n" +
        "    <Property Name=\"FontName\" Type=\"string\" />\n" +
        "    <Property Name=\"FontSize\" Type=\"int\" />\n" +
        "    <Property Name=\"FontStyle\" Type=\"string\" />\n" +
        "  </Component>\n" +
        "</RefXML>";

    #endregion
}