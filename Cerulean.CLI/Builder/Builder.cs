
using Cerulean.Common;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Cerulean.CLI;

public class Builder : IBuilder
{
    private readonly Dictionary<string, IElementHandler> _handlers;
    private readonly List<(XElement, IBuilderContext)> _layouts;
    private readonly List<(XElement, IBuilderContext)> _styles;
    private readonly Dictionary<string, string> _exportedLayouts;
    private readonly Dictionary<string, string> _exportedStyles;
    private readonly Dictionary<string, IBuilderContext> _sheets;
    private readonly List<ComponentRef> _references;

    public IReadOnlyDictionary<string, string> ExportedLayouts => _exportedLayouts;
    public IReadOnlyDictionary<string, string> ExportedStyles => _exportedStyles;
    public IReadOnlyDictionary<string, IBuilderContext> Sheets => _sheets;
    public IReadOnlyList<IComponentRef> ComponentReferences => _references;

    public Builder()
    {
        var config = Config.GetConfig();
        _handlers = new Dictionary<string, IElementHandler>();
        _references = new List<ComponentRef>();
        _layouts = new List<(XElement, IBuilderContext)>();
        _styles = new List<(XElement, IBuilderContext)>();
        _exportedLayouts = new Dictionary<string, string>();
        _exportedStyles = new Dictionary<string, string>();
        _sheets = new Dictionary<string, IBuilderContext>();
        RegisterHandlers();
        RegisterReferences();
        ColoredConsole.Debug($"[$green^DEV$r^] Loaded Special Element Handlers: $cyan^{_handlers.Count}$r^ ($cyan^+1 including GeneralElementHandler$r^).");
        ColoredConsole.Debug($"[$green^DEV$r^] Loaded ComponentRefs: $cyan^{_references.Count}$r^.");
    }

    public bool LexContentFromXml(IBuilderContext context, string xmlFilePath)
    {
        var xml = XDocument.Load(xmlFilePath);
        if (xml.Root is null)
            return false;

        var localId = xml.Root.Attribute("Scope")?.Value ??
                            xml.Root.Attribute("LocalId")?.Value ??
                            GenerateAnonymousName("XML_");

        context.LocalId = localId;
        ColoredConsole.Debug($"[$green^DEV$r^] XML: $cyan^{xmlFilePath}$r^, localId: $yellow^{localId}$r^");

        if (Sheets.ContainsKey(localId))
        {
            ColoredConsole.WriteLine($"[$red^ERROR$r^] Project already contains another XML sheet with the same scope id! ($yellow^{localId}$r^)");
            return false;
        }

        var includes = xml.Root.Elements("Include").ToList();
        includes.ForEach(include =>
        {
            var includeStrings = include.Value.Split('\n',
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var includeValue in includeStrings)
            {
                if (!context.Imports.Contains(includeValue.Trim()))
                    context.Imports.Add(includeValue.Trim());
            }
        });

        var includedStylesheets = xml.Root.Elements("UseScope").ToList();
        includedStylesheets.ForEach(includeScope =>
        {
            var includeStrings = includeScope.Value.Split('\n',
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var includeValue in includeStrings)
            {
                if (!context.ImportedSheets.Contains(includeValue.Trim()))
                    context.ImportedSheets.Add(includeValue.Trim());
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

        _layouts.AddRange(localLayouts.Select(xElement => (xElement, context)));
        _styles.AddRange(localStyles.Select(xElement => (xElement, context)));

        ColoredConsole.Debug($"[$green^DEV$r^] XML: $cyan^{xmlFilePath}$r^, layouts: $yellow^{localLayouts.Length}$r^, styles: $yellow^{localStyles.Length}$r^");

        context.IsStylesheet = localLayouts.Length == 0 && localStyles.Length > 0;
        _sheets.Add(localId, context);

        ColoredConsole.Debug($"[$green^DEV$r^] XML: $cyan^{xmlFilePath}$r^, isStylesheet: $yellow^{context.IsStylesheet}$r^");

        return true;
    }

    public void Build()
    {
        _styles.ForEach(style => ProcessStyle(style.Item1, style.Item2));
        _layouts.ForEach(layout => ProcessLayout(layout.Item1, layout.Item2));
    }

    public void ProcessXElement(IBuilderContext context, StringBuilder stringBuilder, int depth, XElement element, string parent = "")
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
        var target = element.Parent?.Attribute("Target")?.Value;
        if (property is null)
        {
            ColoredConsole.WriteLine(
                $"[$cyan^Style.Setter$r^] [$red^WARN$r^] Setter does not have a 'Name' or 'Property' attribute.");
            return;
        }
        if (target is null)
        {
            ColoredConsole.WriteLine(
                $"[$cyan^Style.Setter$r^] [$red^WARN$r^] Style parent does not have a 'Target' attribute.");
            return;
        }

        var rawValue = element.Attribute("Value")?.Value ?? element.Value;
        var componentRef = ComponentReferences.FirstOrDefault(c => c?.ComponentName == target);
        var type = componentRef?.GetType(property, out _) ?? TypeHelper.GetRecommendedDataType(property, out _);
        var value = TypeHelper.ParseHintedString(rawValue, string.Empty, type);

        stringBuilder.AppendIndented(depth, $"AddSetter(\"{property}\", {value});\n");
    }

    public string GenerateAnonymousName(string? prefix = null)
    {
        const string defaultPrefix = "AnonymousComponent_";
        return $"{prefix ?? defaultPrefix}{DateTime.Now.Ticks}";
    }

    public bool IsComponentFromNamespace(string componentType, string namespacePart)
    {
        return _references.Any(
            reference =>
                string.Equals(componentType, reference.ComponentName) &&
                string.Equals(namespacePart, reference.Namespace)
            );
    }

    public int CountReferences()
    {
        return _references.Count;
    }

    private void ProcessLayout(XElement layoutElement, IBuilderContext context)
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
        var allGlobalStyles = new List<(string, string?)>();
        foreach (var externalSheet in context.ImportedSheets)
        {
            if (Sheets.TryGetValue(externalSheet, out var externalContext))
            {
                allGlobalStyles.AddRange(externalContext.ApplyAsGlobalStyles);
            }
        }
        allGlobalStyles.AddRange(context.ApplyAsGlobalStyles);

        foreach (var (styleName, targetType) in allGlobalStyles)
        {
            // only allow null or "Layout" type
            if (targetType is { } or "Layout")
                continue;
            var queueStyle =
                $"QueueStyle(this, styles.FetchStyle(\"{styleName}\", \"{context.LocalId}{importedSheets}\"));\n";
            stringBuilder.AppendIndented(3, queueStyle);
        }

        // specified styles
        const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries;
        foreach (var styleName in styles.Split(';', options))
        {
            var queueStyle =
                $"QueueStyle(this, styles.FetchStyle(\"{styleName.Trim()}\", \"{context.LocalId}{importedSheets}\"));\n";
            stringBuilder.AppendIndented(3, queueStyle);
        }

        // process layout child elements
        foreach (var xElement in layoutElement.Elements())
            ProcessXElement(context, stringBuilder, 3, xElement);

        Snippets.WriteCtorFooter(stringBuilder);
        Snippets.WriteClassFooter(stringBuilder);

        _exportedLayouts.Add(layoutName, stringBuilder.ToString());
    }

    private void ProcessStyle(XElement styleElement, IBuilderContext context)
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
        _exportedStyles.Add(styleName, stringBuilder.ToString());
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

    private void RegisterReferences()
    {
        var modulesDir = Path.Join(Environment.CurrentDirectory, ".modules");
        var externalAssemblies = new List<Assembly>();

        if (Directory.Exists(modulesDir))
        {
            ColoredConsole.Debug("[$green^DEV$rs^] Loading external assemblies from '.modules'...");
            var dllFiles = new DirectoryInfo(modulesDir).EnumerateFiles("*.dll");
            externalAssemblies.AddRange(
                dllFiles.Select(
                    dll =>
                    {
                        ColoredConsole.Debug($"[$green^DEV$rs^] Loading file {dll.FullName}...");
                        return Assembly.Load(File.ReadAllBytes(dll.FullName));
                    })
            );
        }

        ColoredConsole.Debug($"[$green^DEV$rs^] Loading Components...");
        var componentType = typeof(Component);
        var components = AppDomain.CurrentDomain.GetAssemblies()
            .Union(externalAssemblies)
            .SelectMany(assembly => assembly.GetLoadableTypes())
            .Where(x => x is not null
                        && x != componentType
                        && x.IsAssignableTo(componentType)
                        && x != typeof(object))
            .ToList();

        components.ForEach(component =>
        {
            if (component is not null)
                _references.Add(GenerateReference(component));
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

    private static ComponentRef GenerateReference(Type component)
    {
        var properties = component.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        var componentRef = new ComponentRef
        {
            ComponentName = component.Name,
            Namespace = component.Namespace ?? "Cerulean.App"
        };
        ColoredConsole.Debug($"Generating reference for {component.FullName}...");
        foreach (var property in properties)
        {
            var propName = property.Name;
            var propType = RemoveNullableType(property.PropertyType.FullName);
            if (property.PropertyType.IsEnum)
            {
                propType = $"enum<{propType}>";
            }

            if (property.PropertyType.IsPrimitive || propType is "System.String")
            {
                propType = propType switch
                {
                    "System.Boolean" => "bool",
                    "System.Char" => "char",
                    "System.Byte" => "byte",
                    "System.Int16" => "short",
                    "System.UInt16" => "ushort",
                    "System.Int32" => "int",
                    "System.UInt32" => "uint",
                    "System.Int64" => "long",
                    "System.UInt64" => "ulong",
                    "System.String" => "string",
                    "System.Single" => "float",
                    "System.Double" => "double",
                    "System.Decimal" => "decimal",
                    _ => propType
                };
            }
            var componentTypeAttribute = property.GetCustomAttribute<ComponentTypeAttribute>();
            if (componentTypeAttribute is not null)
            {
                propType = $"component<{componentTypeAttribute.TypeHint}>" +
                           (componentTypeAttribute.IsLateBound ? "*" : "");
            }
            if (propType is null)
            {
                ColoredConsole.Debug($"Component {component.Name}.{propName} has no type!");
                continue;
            }
            componentRef.AddType(propName, propType);
        }

        return componentRef;
    }

    public static string? RemoveNullableType(string? nullableTypeString)
    {
        if (nullableTypeString == null)
            return null;
        var match = Regex.Match(nullableTypeString, "^System.Nullable`1\\[\\[(.+), (.+), (.+), (.+), (.+)\\]\\]$");
        return match.Success ? match.Groups[1].Value : nullableTypeString;
    }
}