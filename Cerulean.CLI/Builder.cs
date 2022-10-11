﻿using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Commands;
using Cerulean.CLI.Extensions;
using Cerulean.Common;

namespace Cerulean.CLI;

public class BuilderContext
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
        ApplyAsGlobalStyles = new List<(string?, string?)>();
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

public class Builder
{
    private readonly Dictionary<string, IElementHandler> _handlers;
    private readonly List<(XElement, BuilderContext)> _layouts;
    private readonly List<(XElement, BuilderContext)> _styles;

    public IDictionary<string, string> ExportedLayouts { get; }
    public IDictionary<string, string> ExportedStyles { get; }
    public IDictionary<string, BuilderContext> Sheets { get; }

    public Builder()
    {
        _handlers = new Dictionary<string, IElementHandler>();
        _layouts = new List<(XElement, BuilderContext)>();
        _styles = new List<(XElement, BuilderContext)>();
        ExportedLayouts = new Dictionary<string, string>();
        ExportedStyles = new Dictionary<string, string>();
        Sheets = new Dictionary<string, BuilderContext>();
        RegisterHandlers();
    }

    public bool LexContentFromXml(BuilderContext context, string xmlFilePath)
    {
        var xml = XDocument.Load(xmlFilePath);
        if (xml.Root is null)
            return false;

        var localId = xml.Root.Attribute("Scope")?.Value ??
                            xml.Root.Attribute("LocalId")?.Value ??
                            GenerateAnonymousName("XML_");

        context.LocalId = localId;

        ColoredConsole.WriteLine($"[$green^DEV$r^] XML: $cyan^{xmlFilePath}$r^, localId: $yellow^{localId}$r^");

        var includes = xml.Root.Elements("Include").ToList();
        includes.ForEach(include =>
        {
            var includeStrings = include.Value.Split('\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var includeValue in includeStrings)
            {
                Console.WriteLine("Adding? {0}", includeValue);
                if (!context.Imports.Contains(includeValue))
                    context.Imports.Add(includeValue);
            }
        });

        var includedStylesheets = xml.Root.Elements("UseScope").ToList();
        includedStylesheets.ForEach(includeScope =>
        {
            var includeStrings = includeScope.Value.Split('\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var includeValue in includeStrings)
            {
                Console.WriteLine("Adding? {0}", includeValue);
                if (!context.ImportedSheets.Contains(includeValue))
                    context.ImportedSheets.Add(includeValue);
            }
        });

        var aliases = xml.Root.Elements("Alias").ToList();
        aliases.ForEach(alias =>
        {
            var aliased = alias.Attribute("Name");
            if (aliased is null)
                return;
            var fullName = alias.Value.Replace(";", "");
            context.Aliases[aliased.Value] = fullName;
        });

        var localLayouts = xml.Root.Elements("Layout").ToArray();
        var localStyles = xml.Root.Elements("Style").ToArray();
        
        _layouts.AddRange(localLayouts.Select( xElement => (xElement, context)));
        _styles.AddRange(localStyles.Select( xElement => (xElement, context)));
        
        ColoredConsole.WriteLine($"[$green^DEV$r^] XML: $cyan^{xmlFilePath}$r^, layouts: $yellow^{localLayouts.Length}$r^, styles: $yellow^{localStyles.Length}$r^");
        context.IsStylesheet = localLayouts.Length == 0 && localStyles.Length > 0;
        if (context.IsStylesheet)
        {
            if (Sheets.ContainsKey(localId))
            {
                ColoredConsole.WriteLine($"[$red^Error$r^] Project already contains a stylesheet called \"{localId}\".");
                return false;
            }
            Sheets.Add(localId, context);
        }

        ColoredConsole.WriteLine($"[$green^DEV$r^] XML: $cyan^{xmlFilePath}$r^, isStylesheet: $yellow^{context.IsStylesheet}$r^");

        return true;
    }

    public void BuildContext()
    {
        _styles.ForEach(style => ProcessStyle(style.Item1, style.Item2));
        _layouts.ForEach(layout => ProcessLayout(layout.Item1, layout.Item2));
    }

    public void ProcessXElement(BuilderContext context, StringBuilder stringBuilder, int depth, XElement element, string parent = "")
    {
        var generalHandler = new GeneralElementHandler();

        var type = element.Name.LocalName;
        if (!_handlers.TryGetValue(type, out var handler))
            handler = generalHandler;

        var result = handler.EvaluateIntoCode(stringBuilder, depth, element, this, context, parent);

        if (!result)
            ColoredConsole.WriteLine(
                $"[$cyan^{handler.GetType()}$r^] Handler returned an error while parsing an XElement.");
    }

    private void ProcessSetter(StringBuilder stringBuilder, int depth, XElement element)
    {
        var property = element.Attribute("Name")?.Value ?? element.Attribute("Property")?.Value;
        if (property is null)
        {
            ColoredConsole.WriteLine(
                $"[$cyan^Style.Setter$r^] [$red^WARN$r^] Setter does not have a 'Name' or 'Property' attribute.");
            return;
        }

        var rawValue = element.Attribute("Value")?.Value ?? element.Value;
        var type = Helper.GetRecommendedDataType(this, property, out var enumFamily, out _);
        var value = Helper.ParseHintedString(rawValue, string.Empty, enumFamily, type);

        stringBuilder.AppendIndented(depth, $"AddSetter(\"{property}\", {value});\n");
    }

    public static string GenerateAnonymousName(string? prefix = null)
    {
        const string defaultPrefix = "AnonymousComponent_";
        return $"{prefix ?? defaultPrefix}{DateTime.Now.Ticks}";
    }

    private void ProcessLayout(XElement layoutElement, BuilderContext context)
    {
        var layoutName = layoutElement.Attribute("Name")?.Value;
        if (string.IsNullOrEmpty(layoutName))
            return;
        var styles = layoutElement.Attribute("Style")?.Value ?? string.Empty;

        var stringBuilder = new StringBuilder();
        Snippets.WriteClassHeader(stringBuilder, layoutName, context.Imports, context.Aliases, "Layout");
        Snippets.WriteCtorHeader(stringBuilder, layoutName, true);

        // global styles
        var importedSheets = string.Join(';', context.ImportedSheets);
        if (importedSheets != string.Empty)
            importedSheets = ";" + importedSheets;

        foreach (var (styleName, targetType) in context.ApplyAsGlobalStyles)
        {
            // only allow null or "Layout" type
            if (targetType is { } or "Layout")
                continue;
            var queueStyle =
                $"QueueStyle(this, styles.FetchStyle(\"{styleName}\", \"{context.LocalId}{importedSheets}\"));\n";
            stringBuilder.AppendIndented(3, queueStyle);
        }

        // specified styles
        const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        foreach (var styleName in styles.Split(';', options))
        {
            var queueStyle =
                $"QueueStyle(this, styles.FetchStyle(\"{styleName}\", \"{context.LocalId}{importedSheets}\"));\n";
            stringBuilder.AppendIndented(3, queueStyle);
        }

        // process layout child elements
        foreach (var xElement in layoutElement.Elements())
            ProcessXElement(context, stringBuilder, 3, xElement);

        Snippets.WriteCtorFooter(stringBuilder);
        Snippets.WriteClassFooter(stringBuilder);

        ExportedLayouts.Add(layoutName, stringBuilder.ToString());
    }

    private void ProcessStyle(XElement styleElement, BuilderContext context)
    {
        var styleName = styleElement.Attribute("Name")?.Value ?? GenerateAnonymousName("Style_");
        var target = styleElement.Attribute("Target")?.Value;
        var hasSelfFlag = bool.TryParse(styleElement.Attribute("ApplyToSelf")?.Value, out var applyToSelf);
        var hasChildFlag = bool.TryParse(styleElement.Attribute("ApplyToChildren")?.Value, out var applyToChildren);
        var derivedFrom = styleElement.Attribute("DerivedFrom")?.Value ?? styleElement.Attribute("From")?.Value;

        var stringBuilder = new StringBuilder();

        if (target?.EndsWith('*') == true || styleElement.Attribute("Name") == null)
        {
            if (target?.EndsWith('*') == true)
                target = target[..^1];
            context.ApplyAsGlobalStyles.Add((styleName, target));
        }

        var attributes = new[]
        {
            context.IsStylesheet
                ?  "[Scope(StyleScope.Global)]"
                : $"[Scope(StyleScope.Local, \"{context.LocalId}\")]"
        };
        Snippets.WriteClassHeader(stringBuilder, styleName, context.Imports, context.Aliases, "Style", false, attributes);
        Snippets.WriteCtorHeader(stringBuilder, styleName, true);
        if (target is not null)
            stringBuilder.AppendIndented(3, $"TargetType = typeof({target});\n");
        if (hasSelfFlag)
            stringBuilder.AppendIndented(3, $"ApplyToSelf = {applyToSelf.ToLowerString()};\n");
        if (hasChildFlag)
            stringBuilder.AppendIndented(3, $"ApplyToChildren = {applyToChildren.ToLowerString()};\n");
        if (derivedFrom is not null)
            stringBuilder.AppendIndented(3, $"DeriveFrom(styles.FetchStyle(\"{derivedFrom}\"));\n");
        foreach (var xElement in styleElement.Elements("Setter"))
            ProcessSetter(stringBuilder, 3, xElement);
        Snippets.WriteCtorFooter(stringBuilder);
        Snippets.WriteClassFooter(stringBuilder);
        ExportedStyles.Add(styleName, stringBuilder.ToString());
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