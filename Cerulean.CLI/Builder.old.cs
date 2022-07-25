using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Cerulean.CLI.Extensions;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class BuilderContextOld
    {
        public IDictionary<string, string> Layouts { get; init; }
        public IDictionary<string, string> Styles { get; init; }
        public IList<string> Imports { get; init; }
        public IDictionary<string, string> Aliases { get; init; }

        public BuilderContext()
        {
            Layouts = new Dictionary<string, string>();
            Styles = new Dictionary<string, string>();
            Imports = new List<string>();
            Aliases = new Dictionary<string, string>();
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
    public static class BuilderOld
    {
        public static string ParseNestedComponentName(string nestedName, string root)
        {
            StringBuilder component = new();
            var nests = nestedName.Split('.');
            component.Append($"{root}GetChild(\"{nests[0]}\")");
            for (var i = 1; i < nests.Length; i++)
            {
                component.Append($".GetChild(\"{nests[i]}\")");
            }
            return component.ToString();
        }

        public static string Repeat(string str, int repetition)
        {
            StringBuilder builder = new();
            for (var i = 0; i < repetition; ++i)
                builder.Append(str);
            return builder.ToString();
        }

        public static int FromHex(string str)
            => int.Parse(str, NumberStyles.HexNumber);

        public static string? GetRecommendedDataType(string propertyName, out string? enumFamily)
        {
            var type = propertyName switch
            {
                "ForeColor" => "color",
                "BackColor" => "color",
                "BorderColor" => "color",
                "FontName" => "string",
                "FontSize" => "int",
                "FontStyle" => "string",
                "Text" => "string",
                "FileName" => "string",
                "X" => "int",
                "Y" => "int",
                "PictureMode" => "enum",
                _ => null
            };
            enumFamily = propertyName switch
            {
                "TargetMouseButton" => "MouseButton",
                _ => propertyName
            };
            return type;
        }

        public static string ParseHexColor(string hexColor)
        {
            var color = "Colors.None";
            // #RGB
            var pattern1 = Regex.Match(hexColor, @"^#([\da-f])([\da-f])([\da-f])$", RegexOptions.IgnoreCase);
            if (pattern1.Success)
            {
                var r = FromHex(Repeat(pattern1.Groups[1].Value, 2));
                var g = FromHex(Repeat(pattern1.Groups[2].Value, 2));
                var b = FromHex(Repeat(pattern1.Groups[3].Value, 2));
                color = $"new Color({r}, {g}, {b})";
            }

            // #RGBA
            var pattern2 = Regex.Match(hexColor, @"^#([\da-f])([\da-f])([\da-f])([\da-f])$", RegexOptions.IgnoreCase);
            if (pattern2.Success)
            {
                var r = FromHex(Repeat(pattern2.Groups[1].Value, 2));
                var g = FromHex(Repeat(pattern2.Groups[2].Value, 2));
                var b = FromHex(Repeat(pattern2.Groups[3].Value, 2));
                var a = FromHex(Repeat(pattern2.Groups[4].Value, 2));
                color = $"new Color({r}, {g}, {b}, {a})";
            }

            // #RRGGBB
            var pattern3 = Regex.Match(hexColor, @"^#([\da-f]{2})([\da-f]{2})([\da-f]{2})$", RegexOptions.IgnoreCase);
            if (pattern3.Success)
            {
                var r = FromHex(pattern3.Groups[1].Value[..2]);
                var g = FromHex(pattern3.Groups[2].Value[..2]);
                var b = FromHex(pattern3.Groups[3].Value[..2]);
                color = $"new Color({r}, {g}, {b})";
            }

            // #RRGGBBAA
            var pattern4 = Regex.Match(hexColor, @"^#([\da-f]{2})([\da-f]{2})([\da-f]{2})([\da-f]{2})$", RegexOptions.IgnoreCase);
            if (pattern4.Success)
            {
                var r = FromHex(pattern4.Groups[1].Value[..2]);
                var g = FromHex(pattern4.Groups[2].Value[..2]);
                var b = FromHex(pattern4.Groups[3].Value[..2]);
                var a = FromHex(pattern4.Groups[4].Value[..2]);
                color = $"new Color({r}, {g}, {b}, {a})";
            }

            return color;
        }

        public static string ParseHintedString(string hintedString, string root, string? enumFamiy = null, string? overrideType = null)
        {
            // hinted value pattern ([name]="[type]: [value]")
            var regex = Regex.Match(hintedString, @"^(\w+):\s?(.+)");
            var value = hintedString;

            // attribute value-part has datatype hint
            if (regex.Success || overrideType is not null)
            {
                var type = overrideType ?? regex.Groups[1].ToString().ToLower();
                var raw = overrideType is null ? regex.Groups[2].ToString() : hintedString;
                try
                {
                    value = type switch
                    {
                        "bool" => $"{bool.Parse(raw)}",
                        "byte" => $"{byte.Parse(raw)}",
                        "char" => $"'{raw[0]}'",
                        "short" => $"{short.Parse(raw)}",
                        "ushort" => $"{ushort.Parse(raw)}",
                        "int" => $"{int.Parse(raw)}",
                        "uint" => $"{uint.Parse(raw)}",
                        "long" => $"{long.Parse(raw)}",
                        "ulong" => $"{ulong.Parse(raw)}",
                        "float" => $"{float.Parse(raw)}",
                        "double" => $"{double.Parse(raw)}",
                        "string" => $"\"{raw}\"",
                        "component" => ParseNestedComponentName(raw, root),
                        "color" => ParseHexColor(raw),
                        "literal" => value,
                        "enum" => $"{enumFamiy}.{raw}",
                        _ => "null"
                    };
                    switch (value)
                    {
                        case "null":
                            Console.WriteLine($"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Type '{type}' for attribute '{hintedString}' not recognized, using null instead.");
                            break;
                        case "Colors.None" when type == "color":
                            Console.WriteLine($"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Color '{hintedString}' not parsed correctly, using Colors.None instead.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Could not cast type '{type}' for attribute '{hintedString}': {raw}, using null instead. {ex.Message}");
                    value = "null";
                }

            }
            return value;
        }
        public static bool BuildContextFromXML(BuilderContext context, string xmlFilePath)
        {
            try
            {
                var xml = XDocument.Load(xmlFilePath);
                var styles = xml.Root?.Elements("Style").ToList();
                var layouts = xml.Root?.Elements("Layout").ToList();
                var includes = xml.Root?.Elements("Include").ToList();
                var aliases = xml.Root?.Elements("Alias").ToList();
                if (styles is null || layouts is null || includes is null || aliases is null)
                    return false;
                foreach (var include in includes)
                {
                    var import = include.Value.Replace(";", string.Empty);
                    if (!context.Imports.Contains(import))
                        context.Imports.Add(import);
                }
                foreach (var alias in aliases)
                {
                    var name = alias.Attribute("Name")?.Value;
                    if (name is null)
                    {
                        ColoredConsole.WriteLine("[$yellow^WARN$r^][$yellow^ALIAS$r^] Alias has no 'Name' attribute, ignoring...");
                        continue;
                    }
                    var value = alias.Value.Replace(";", "");
                    if (value == string.Empty)
                    {
                        ColoredConsole.WriteLine($"[$yellow^WARN$r^][$yellow^ALIAS$r^] Alias '{name}' has an empty value, ignoring...");
                        continue;
                    }
                    context.Aliases[name] = value;
                }
                foreach (var layout in layouts)
                {
                    try
                    {
                        var name = GenerateLayout(context, layout, out var content);
                        context.Layouts.Add(name, content);
                        ColoredConsole.WriteLine($"[$green^GOOD$r^][$yellow^LAYOUT$r^] Generated layout '{name}'.");
                    }
                    catch (Exception e)
                    {
                        ColoredConsole.WriteLine("[$red^FAIL$r^][$yellow^LAYOUT$r^] Failed generating layout for XML-Fragment:");
                        if (((IXmlLineInfo)layout).HasLineInfo())
                            Console.WriteLine(@"Line: {0}, Col: {1}", ((IXmlLineInfo)layout).LineNumber, ((IXmlLineInfo)layout).LinePosition);
                        Console.WriteLine("{0}", layout.ToString());
                        ColoredConsole.WriteLine($"[$red^FAIL$r^][REASON] {e.Message}");
                    }
                    Console.WriteLine();
                }
                foreach (var style in styles)
                {
                    try
                    {
                        var name = GenerateStyle(context, style, out var content);
                        context.Styles.Add(name, content);
                        ColoredConsole.WriteLine($"[$green^GOOD$r^][$yellow^STYLE$r^] Generated style '{name}'.");
                    }
                    catch (Exception)
                    {
                        ColoredConsole.WriteLine("[$red^FAIL$r^][$yellow^STYLE$r^] Failed generating style for XML-Fragment.");
                        if (((IXmlLineInfo)style).HasLineInfo())
                            Console.WriteLine(@"Line: {0}, Col: {1}", ((IXmlLineInfo)style).LineNumber, ((IXmlLineInfo)style).LinePosition);
                        Console.WriteLine("{0}", style.ToString());
                        ColoredConsole.WriteLine("[$red^FAIL$r^][REASON] {e.Message}");
                    }
                    Console.WriteLine();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static string GenerateStyle(BuilderContext context, XElement element, out string content)
        {
            throw new NotImplementedException();
            //var name = element.Attribute("Name")?.Value;
            //var target = element.Attribute("Target")?.Value;
            //if (name is null)
            //    throw new InvalidDataException("Style does not have a 'Name' attribute.");
            //StringBuilder source = new();
            //GenerateHeader(context, source, name, "Style");
            //foreach (var setter in element.Elements("Setter"))
            //{

            //}
            //GenerateFooter(source);
        }
        private static string GenerateLayout(BuilderContext context, XElement element, out string content)
        {
            var name = element.Attribute("Name")?.Value;
            if (name is null)
                throw new InvalidDataException("Layout does not have a 'Name' attribute.");
            var parent = element.Attribute("Base")?.Value ?? "Layout";
            StringBuilder source = new();

            GenerateHeader(context, source, name, parent);
            source.AppendIndented(2, $"public {name}() : base()\n");
            source.AppendIndented(2, "{\n");
            GenerateChildElements(3, source, element);
            source.AppendIndented(2, "}\n");
            GenerateFooter(source);

            content = source.ToString();
            return name;
        }
        private static void GenerateHeader(BuilderContext context, StringBuilder stringBuilder, string name, string? parent = null)
        {
            // create imports
            foreach (var import in context.Imports)
            {
                stringBuilder.Append($"using {import};\n");
            }
            // create aliases
            foreach (var alias in context.Aliases)
            {
                stringBuilder.Append($"using {alias.Key} = {alias.Value};\n");
            }
            stringBuilder.Append("#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.\n");
            stringBuilder.Append("#pragma warning disable CS8602 // Dereference of a possibly null reference.\n");
            stringBuilder.Append("// Generated with Cerulean-API Builder\n");
            stringBuilder.Append("namespace Cerulean.App\n");
            stringBuilder.Append("{\n");
            stringBuilder.AppendIndented(1, $"public partial class {name}{(parent is not null ? " : " + parent : "")}\n");
            stringBuilder.AppendIndented(1, "{\n");
        }
        private static void GenerateFooter(StringBuilder stringBuilder)
        {
            stringBuilder.AppendIndented(1, "}\n");
            stringBuilder.Append("}\n");
            stringBuilder.Append("#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.\n");
            stringBuilder.Append("#pragma warning restore CS8602 // Dereference of a possibly null reference.\n");
            stringBuilder.Append($"// Generated on: {DateTime.Now}");
        }
        private static void GenerateChildElements(int indent, StringBuilder stringBuilder, XElement element, string root = "")
        {
            foreach (var child in element.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "Event":
                    {
                        // event is a top level element
                        if (element.Name.ToString() == "Layout")
                        {
                            if (child.Attribute("Name")?.Value is { } eventName &&
                                child.Attribute("Component")?.Value is { } componentName &&
                                child.Attribute("Type")?.Value is { } type &&
                                child.Attribute("Handler")?.Value is { } eventHandler)
                            {
                                if (child.Value == string.Empty)
                                    stringBuilder.AppendIndented(indent, $"(({type}){root}GetChild(\"{componentName}\")).{eventName} += {eventHandler};\n");
                            }
                            else
                            {
                                ColoredConsole.WriteLine("[$yellow^WARN$r^][$yellow^EVENT$r^] Top-level event is malformed, event ignored.");
                                Console.WriteLine("{0}", child);
                                Console.WriteLine();
                            }
                        }
                        continue;
                    }
                    case "Invoke":
                    {
                        // invoke is a top level element
                        if (element.Name.ToString() == "Layout")
                        {
                            if (child.Attribute("Method")?.Value is { } method)
                            {
                                var args = string.Empty;
                                var target = string.Empty;
                                var type = string.Empty;
                                if (child.Attribute("Args")?.Value is { } rawArgs)
                                    args = ParseHintedString(rawArgs, root);
                                if (child.Attribute("Type")?.Value is { } rawValue)
                                    type = $"({rawValue})";
                                if (child.Attribute("Target")?.Value is { } rawTarget)
                                    target = $"{ParseNestedComponentName(rawTarget, root)}";
                                if (type != string.Empty && target != string.Empty)
                                {
                                    stringBuilder.AppendIndented(indent, $"({type}{root}{target}).{method}({args});\n");
                                }
                                else
                                {
                                    if (target != string.Empty)
                                        target += ".";
                                    stringBuilder.AppendIndented(indent, $"{root}{target}{method}({args});\n");
                                }
                            }
                            else
                            {
                                ColoredConsole.WriteLine("[$yellow^WARN$r^][$yellow^INVOKE$r^] Invoke element does not have a 'Method' attribute.");
                                Console.WriteLine("{0}", child);
                                Console.WriteLine();
                            }
                        }
                        continue;
                    }
                    case "Snippet":
                    {
                        // TODO: Make C# Snippet Element
                        continue;
                    }
                }

                var name = child.Attribute("Name")?.Value;
                if (name is null)
                {
                    ColoredConsole.WriteLine("[$yellow$r^WARN][$yellow^LAYOUT$r^] Child element does not have a 'Name' attribute. Element skipped.");
                    if (((IXmlLineInfo)child).HasLineInfo())
                        Console.WriteLine(@"Line: {0}, Col: {1}", ((IXmlLineInfo)child).LineNumber, ((IXmlLineInfo)child).LinePosition);
                    Console.WriteLine("{0}", child.ToString());
                    continue;
                }
                stringBuilder.AppendIndented(indent, $"{root}AddChild(\"{name}\", new {child.Name}({(child.Attribute("Data")?.Value ?? "")})\n");
                stringBuilder.AppendIndented(indent, "{\n");
                var props = child.Attributes().Where(attribute =>
                {
                    return attribute.Name.ToString() != "Name" &&
                           attribute.Name.ToString() != "Data" &&
                           attribute.Name.NamespaceName != "Attribute";
                }).ToArray();
                var attributes = child.Attributes().Where(attribute =>
               {
                   return attribute.Name.NamespaceName == "Attribute";
               }).ToArray();
                foreach (var prop in props)
                {
                    var recommendedType = GetRecommendedDataType(prop.Name.ToString(), out var enumFamily);
                    stringBuilder.AppendIndented(indent + 1, $"{prop.Name} = {ParseHintedString(prop.Value, root, enumFamily, recommendedType)},\n");
                }
                stringBuilder.AppendIndented(indent, "});\n");
                foreach (var attribute in attributes)
                {
                    stringBuilder.AppendIndented(indent, $"{root}GetChild(\"{name}\").AddOrUpdateAttribute(\"{attribute.Name.LocalName}\", {ParseHintedString(attribute.Value, root)});\n");
                }
                var events = child.Elements("Event");
                var invokes = child.Elements("Invoke");
                foreach (var e in events)
                {
                    if (e.Attribute("Name")?.Value is { } eventName)
                    {
                        if (e.Attribute("Handler")?.Value is { } eventHandler)
                            stringBuilder.AppendIndented(indent, $"(({child.Name}){root}GetChild(\"{name}\")).{eventName} += {eventHandler};\n");
                        else
                            ColoredConsole.WriteLine($"[$yellow^WARN$r^][$yellow^EVENT$r^] Event from component '{name}' has no 'Handler' attribute. Ignoring...");
                    }
                    else
                    {
                        ColoredConsole.WriteLine($"[$yellow^WARN$r^][$yellow^EVENT$r^] Event from component '{name}' has no 'Name' attribute. Ignoring...");
                    }
                }
                foreach (var i in invokes)
                {
                    if (i.Attribute("Method")?.Value is { } method)
                    {
                        var args = string.Empty;
                        var target = string.Empty;
                        var type = string.Empty;
                        if (i.Attribute("Args")?.Value is { } rawArgs)
                            args = ParseHintedString(rawArgs, root);
                        if (i.Attribute("Type")?.Value is { } rawValue)
                            type = $"({rawValue})";
                        if (i.Attribute("Target")?.Value is { } rawTarget)
                            target = $".{ParseNestedComponentName(rawTarget, root)}";
                        if (target == string.Empty && type == string.Empty)
                            type = $"({child.Name})";
                        if (type != string.Empty)
                            stringBuilder.AppendIndented(indent, $"({type}{root}GetChild(\"{name}\"){target}).{method}({args});\n");
                        else
                            stringBuilder.AppendIndented(indent, $"{root}GetChild(\"{name}\"){target}.{method}({args});\n");
                        /*
                        if (child.Value != string.Empty)
                            stringBuilder.AppendIndented(indent, $"(({type})GetChild(\"{componentName}\")).{eventName} += {eventHandler};\n");
                        */
                    }
                    else
                    {
                        ColoredConsole.WriteLine("[$yellow^WARN$r^][$yellow^INVOKE$r^] Invoke element does not have a 'Method' attribute.");
                        Console.WriteLine("{0}", child);
                        Console.WriteLine();
                    }
                }
                GenerateChildElements(indent, stringBuilder, child, $"{root}GetChildNullable(\"{name}\")?.");
            }
        }
    }
}
