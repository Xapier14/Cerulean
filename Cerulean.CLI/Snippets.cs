using System.Diagnostics;
using System.Reflection;
using System.Text;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI;

internal static class Snippets
{
    public static readonly string VersionString = InitializeVersionString();

    private static string InitializeVersionString()
    {
        try
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            return fileVersionInfo.FileVersion ?? string.Empty;
        }
        catch (Exception)
        {
            // ignore when fail, use empty string instead.
            return string.Empty;
        }
    }

    public static void WriteClassHeader(StringBuilder stringBuilder, string className,
        IEnumerable<string> importStrings, IDictionary<string, string> aliasStrings, string? parent = null, bool isPartial = true,
        IEnumerable<string>? attributes = null)
    {
        foreach (var import in importStrings)
            stringBuilder.AppendLine($"using {import};");
        foreach (var alias in aliasStrings)
            stringBuilder.AppendLine($"using {alias.Key} = {alias.Value};");
        stringBuilder.AppendLine(
            "#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.");
        stringBuilder.AppendLine("#pragma warning disable CS8602 // Dereference of a possibly null reference.");
        stringBuilder.AppendLine("// Generated with Cerulean.CLI " + VersionString);
        stringBuilder.AppendLine("namespace Cerulean.App");
        stringBuilder.AppendLine("{");
        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                stringBuilder.AppendIndented(1, $"{attribute}\n");
            }
        }
        stringBuilder.AppendIndented(1,
            $"public {(isPartial ? "partial " : string.Empty)}class {className}{(parent is not null ? " : " + parent : "")}\n");
        stringBuilder.AppendIndented(1, "{\n");
    }

    public static void WriteCtorHeader(StringBuilder stringBuilder, string className, bool hasInheritance = false)
    {
        var baseSuffix = hasInheritance ? " : base()" : string.Empty;
        stringBuilder.AppendIndented(2, $"public {className}(){baseSuffix}\n");
        stringBuilder.AppendIndented(2, "{\n");
        stringBuilder.AppendIndented(3, "var api = CeruleanAPI.GetAPI();\n");
        stringBuilder.AppendIndented(3, "var resources = api.EmbeddedResources;\n");
        stringBuilder.AppendIndented(3, "var styles = api.EmbeddedStyles;\n");
    }

    public static void WriteCtorFooter(StringBuilder stringBuilder)
    {
        stringBuilder.AppendIndented(2, "}\n");
    }

    public static void WriteClassFooter(StringBuilder stringBuilder)
    {
        stringBuilder.AppendIndented(1, "}\n");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine(
            "#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.");
        stringBuilder.AppendLine("#pragma warning restore CS8602 // Dereference of a possibly null reference.");
        stringBuilder.AppendLine($"// Generated on: {DateTime.Now}");
    }
}