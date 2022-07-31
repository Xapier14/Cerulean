using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI;

public class BuilderContext
{
    public BuilderContext()
    {
        Layouts = new Dictionary<string, string>();
        Styles = new Dictionary<string, string>();
        Imports = new List<string>();
        Aliases = new Dictionary<string, string>();
    }

    public IDictionary<string, string> Layouts { get; init; }
    public IDictionary<string, string> Styles { get; init; }
    public IList<string> Imports { get; init; }
    public IDictionary<string, string> Aliases { get; init; }

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

public class Builder
{
    private readonly IDictionary<string, IElementHandler> _handlers;

    public Builder()
    {
        _handlers = new Dictionary<string, IElementHandler>();
        RegisterHandlers();
    }

    public bool BuildContextFromXml(BuilderContext context, string xmlFilePath)
    {
        var xml = XDocument.Load(xmlFilePath);
        if (xml.Root is null)
            return false;

        var layouts = xml.Root.Elements("Layout").ToList();
        var styles = xml.Root.Elements("Style").ToList();
        var includes = xml.Root.Elements("Include").ToList();
        var aliases = xml.Root.Elements("Alias").ToList();

        includes.ForEach(include =>
        {
            if (!context.Imports.Contains(include.Value))
                context.Imports.Add(include.Value);
        });
        aliases.ForEach(alias =>
        {
            var aliased = alias.Attribute("Name");
            if (aliased is null)
                return;
            var fullName = alias.Value.Replace(";", "");
            context.Aliases[aliased.Value] = fullName;
        });
        layouts.ForEach(x => ProcessLayout(x, context));
        styles.ForEach(x => ProcessStyle(x, context));

        return true;
    }

    public void ProcessXElement(StringBuilder stringBuilder, int depth, XElement element, string parent = "")
    {
        var generalHandler = new GeneralElementHandler();

        var type = element.Name.LocalName;
        if (!_handlers.TryGetValue(type, out var handler))
            handler = generalHandler;

        var result = handler.EvaluateIntoCode(stringBuilder, depth, element, this, parent);

        if (!result)
            ColoredConsole.WriteLine(
                $"[$cyan^{handler.GetType()}$r^] Handler returned an error while parsing an XElement.");
    }

    public static string GenerateAnonymousName()
    {
        const string prefix = " _.";
        return $"{prefix}{DateTime.Now.Ticks}_Component";
    }

    private void ProcessLayout(XElement layoutElement, BuilderContext context)
    {
        var layoutName = layoutElement.Attribute("Name")?.Value;
        if (string.IsNullOrEmpty(layoutName))
            return;

        var stringBuilder = new StringBuilder();
        Snippets.WriteClassHeader(stringBuilder, layoutName, context.Imports, context.Aliases, "Layout");
        Snippets.WriteCtorHeader(stringBuilder, layoutName, true);
        foreach (var xElement in layoutElement.Elements()) ProcessXElement(stringBuilder, 3, xElement);
        Snippets.WriteCtorFooter(stringBuilder);
        Snippets.WriteClassFooter(stringBuilder);

        context.Layouts.Add(layoutName, stringBuilder.ToString());
    }

    private void ProcessStyle(XElement styleElement, BuilderContext context)
    {
        var styleName = styleElement.Name.LocalName;
        if (styleName == string.Empty)
            return;

        throw new NotImplementedException();
    }

    private void RegisterHandlers()
    {
        var interfaceType = typeof(IElementHandler);
        var handlers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetLoadableTypes())
            .Where(x => x is not null
                        && x != interfaceType
                        && x.IsAssignableTo(interfaceType)
                        && x != typeof(object))
            .ToList();

        handlers.ForEach(handler =>
        {
            string? handlerName = null;
            var attributes = handler?.GetCustomAttributes()?.ToList();
            if (attributes is null)
                return;
            var ignore = false;
            attributes.ForEach(attribute =>
            {
                switch (attribute)
                {
                    case ElementTypeAttribute typeAttribute:
                        handlerName = typeAttribute.ElementType;
                        break;
                    case IgnoreAttribute _:
                        ignore = true;
                        break;
                }
            });
            if (ignore)
                return;

            var constructor = handler?.GetConstructor(Array.Empty<Type>());

            RegisterHandler(handlerName, constructor);
        });
    }

    private void RegisterHandler(string? handlerName, ConstructorInfo? handlerConstructor)
    {
        var instance = handlerConstructor?.Invoke(Array.Empty<object>());
        if (handlerName is not null && instance is not null)
            _handlers[handlerName] = (IElementHandler)instance;
        else
            ColoredConsole.WriteLine(
                $"[$cyan^Builder.RegisterHandler()$r^] Could not load element handler \"{handlerConstructor?.Name}\".");
    }
}