using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Cerulean.Common;

namespace Cerulean.CLI
{
    public class BuilderContext
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
    public static class Builder
    {
        public static string ParseNestedComponentName(string nestedName, string root)
        {
            StringBuilder component = new();
            var nests = nestedName.Split('.');
            component.Append($"{root}GetChild(\"{nests[0]}\")");
            for (int i = 1; i < nests.Length; i++)
            {
                component.Append($".GetChild(\"{nests[i]}\")");
            }
            return component.ToString();
        }
        public static string ParseHintedString(string hintedString, string root)
        {
            // hinted value pattern ([name]="[type]: [value]")
            var regex = Regex.Match(hintedString, @"^(\w+):\s?(.+)");
            string value = hintedString;

            // attribute value-part has datatype hint
            if (regex.Success)
            {
                string type = regex.Groups[1].ToString().ToLower();
                string raw = regex.Groups[2].ToString();
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
                        "component" => $"{ParseNestedComponentName(raw, root)}",
                        "literal" => value,
                        _ => "null"
                    };
                    if (value == "null")
                        Console.WriteLine("[WARN][COMPONENT] Type '{0}' for attribute '{1}' not recognized, using null instead.", type, hintedString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARN][COMPONENT] Could not cast type '{0}' for attribute '{1}': {2}, using null instead. {3}", type, hintedString, raw, ex.Message);
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
                    if (include is null)
                        continue;
                    string import = include.Value.Replace(";", string.Empty);
                    if (!context.Imports.Contains(import))
                        context.Imports.Add(import);
                }
                foreach (var alias in aliases)
                {
                    if (alias is null)
                        continue;
                    string? name = alias.Attribute("Name")?.Value;
                    if (name is null)
                    {
                        Console.WriteLine("[WARN][ALIAS] Alias has no 'Name' attribute, ignoring...");
                        continue;
                    }
                    string value = alias.Value.Replace(";", "");
                    if (value == string.Empty)
                    {
                        Console.WriteLine("[WARN][ALIAS] Alias '{0}' has an empty value, ignoring...", name);
                        continue;
                    }
                    context.Aliases[name] = value;
                }
                foreach (var layout in layouts)
                {
                    try
                    {
                        if (layout is null)
                            continue;
                        string name = GenerateLayout(context, layout, out string content);
                        context.Layouts.Add(name, content);
                        Console.WriteLine("[GOOD][LAYOUT] Generated layout '{0}'.", name);
                    } catch (Exception e)
                    {
                        Console.WriteLine("[FAIL][LAYOUT] Failed generating layout for XML-Fragment:");
                        if (((IXmlLineInfo)layout).HasLineInfo())
                            Console.WriteLine("Line: {0}, Col: {1}", ((IXmlLineInfo)layout).LineNumber, ((IXmlLineInfo)layout).LinePosition);
                        Console.WriteLine("{0}", layout.ToString());
                        Console.WriteLine("[FAIL][REASON] {0}", e.Message);
                    }
                    Console.WriteLine();
                }
                foreach (var style in styles)
                {
                    try
                    {
                        if (style is null)
                            continue;
                        string name = GenerateStyle(context, style, out string content);
                        context.Styles.Add(name, content);
                        Console.WriteLine("[GOOD][STYLE] Generated style '{0}'.", name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[FAIL][STYLE] Failed generating style for XML-Fragment.");
                        if (((IXmlLineInfo)style).HasLineInfo())
                            Console.WriteLine("Line: {0}, Col: {1}", ((IXmlLineInfo)style).LineNumber, ((IXmlLineInfo)style).LinePosition);
                        Console.WriteLine("{0}", style.ToString());
                        Console.WriteLine("[FAIL][REASON] {0}", e.Message);
                    }
                    Console.WriteLine();
                }
                return true;
            } catch (Exception)
            {
                return false;
            }
        }
        private static string GenerateStyle(BuilderContext context, XElement element, out string content)
        {
            string? name = element.Attribute("Name")?.Value;
            string? target = element.Attribute("Target")?.Value;
            if (name is null)
                throw new InvalidDataException("Style does not have a 'Name' attribute.");
            StringBuilder source = new();
            GenerateHeader(context, source, name, "Style");
            foreach (var setter in element.Elements("Setter"))
            {
                if (setter is null)
                    continue;
            }
            GenerateFooter(source);
            throw new NotImplementedException();
        }
        private static string GenerateLayout(BuilderContext context, XElement element, out string content)
        {
            string? name = element.Attribute("Name")?.Value;
            if (name is null)
                throw new InvalidDataException("Layout does not have a 'Name' attribute.");
            string parent = element.Attribute("Base")?.Value ?? "Layout";
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
            foreach (string import in context.Imports)
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
                if (child.Name.ToString() == "Event")
                {
                    // event is a top level element
                    if (element.Name.ToString() == "Layout")
                    {
                        if (child.Attribute("Name")?.Value is string eventName &&
                            child.Attribute("Component")?.Value is string componentName &&
                            child.Attribute("Type")?.Value is string type &&
                            child.Attribute("Handler")?.Value is string eventHandler)
                        {
                            if (child.Value == string.Empty)
                                stringBuilder.AppendIndented(indent, $"(({type}){root}GetChild(\"{componentName}\")).{eventName} += {eventHandler};\n");
                        }
                        else
                        {
                            Console.WriteLine("[WARN][EVENT] Top-level event is malformed, event ignored.");
                            Console.WriteLine("{0}", child);
                            Console.WriteLine();
                        }
                    }
                    continue;
                }
                if (child.Name.ToString() == "Invoke")
                {
                    // invoke is a top level element
                    if (element.Name.ToString() == "Layout")
                    {
                        if (child.Attribute("Method")?.Value is string method)
                        {
                            string args = string.Empty;
                            string target = string.Empty;
                            string type = string.Empty;
                            if (child.Attribute("Args")?.Value is string rawArgs)
                                args = ParseHintedString(rawArgs, root);
                            if (child.Attribute("Type")?.Value is string rawValue)
                                type = $"({rawValue})";
                            if (child.Attribute("Target")?.Value is string rawTarget)
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

                            /*
                            if (child.Value != string.Empty)
                                stringBuilder.AppendIndented(indent, $"(({type})GetChild(\"{componentName}\")).{eventName} += {eventHandler};\n");
                            */
                        }
                        else
                        {
                            Console.WriteLine("[WARN][INVOKE] Invoke element does not have a 'Method' attribute.");
                            Console.WriteLine("{0}", child);
                            Console.WriteLine();
                        }
                    }
                    continue;
                }
                string? name = child.Attribute("Name")?.Value;
                if (name is null)
                {
                    Console.WriteLine("[WARN][LAYOUT] Child element does not have a 'Name' attribute. Element skipped.");
                    if (((IXmlLineInfo)child).HasLineInfo())
                        Console.WriteLine("Line: {0}, Col: {1}", ((IXmlLineInfo)child).LineNumber, ((IXmlLineInfo)child).LinePosition);
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
                var attributes = child.Attributes().Where( attribute =>
                {
                    return attribute.Name.NamespaceName == "Attribute";
                }).ToArray();
                foreach (var prop in props)
                {
                    stringBuilder.AppendIndented(indent + 1, $"{prop.Name} = {ParseHintedString(prop.Value, root)},\n");
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
                    if (e.Attribute("Name")?.Value is string eventName)
                    {
                        if (e.Attribute("Handler")?.Value is string eventHandler)
                                stringBuilder.AppendIndented(indent, $"(({child.Name}){root}GetChild(\"{name}\")).{eventName} += {eventHandler};\n");
                        else
                            Console.WriteLine("[WARN][EVENT] Event from component '{0}' has no 'Handler' attribute. Ignoring...", name);
                    } else
                    {
                        Console.WriteLine("[WARN][EVENT] Event from component '{0}' has no 'Name' attribute. Ignoring...", name);
                    }
                }
                foreach (var i in invokes)
                {
                    if (i.Attribute("Method")?.Value is string method)
                    {
                        string args = string.Empty;
                        string target = string.Empty;
                        string type = string.Empty;
                        if (i.Attribute("Args")?.Value is string rawArgs)
                            args = ParseHintedString(rawArgs, root);
                        if (i.Attribute("Type")?.Value is string rawValue)
                            type = $"({rawValue})";
                        if (i.Attribute("Target")?.Value is string rawTarget)
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
                        Console.WriteLine("[WARN][INVOKE] Invoke element does not have a 'Method' attribute.");
                        Console.WriteLine("{0}", child);
                        Console.WriteLine();
                    }
                }
                GenerateChildElements(indent, stringBuilder, child, $"{root}GetChildNullable(\"{name}\")?.");
            }
        }
    }
}
