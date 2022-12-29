using Cerulean.Common;

namespace Cerulean.CLI;

public class BuilderContext : IBuilderContext
{

    public IList<string> Imports { get; }
    public IList<string> ImportedSheets { get; }
    public IDictionary<string, string> Aliases { get; }
    public IList<(string, string?)> ApplyAsGlobalStyles { get; }
    public string LocalId { get; set; }
    public bool IsStylesheet { get; set; }

    public BuilderContext()
    {
        Imports = new List<string>();
        ImportedSheets = new List<string>();
        Aliases = new Dictionary<string, string>();
        ApplyAsGlobalStyles = new List<(string, string?)>();
        LocalId = string.Empty;
        IsStylesheet = false;
    }

    public void UseDefaultImports()
    {
        // Namespaces
        Imports.Clear();
        Imports.Add("Cerulean");
        Imports.Add("Cerulean.Core");
        Imports.Add("Cerulean.Common");
        Imports.Add("Cerulean.Components");
    }
}