using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public static class TypeHelper
    {
        public static string? GetRecommendedDataType(string propertyName, out bool needsLateBind)
        {
            var type = propertyName switch
            {
                "GridRow" => "int",
                "GridColumn" => "int",
                "GridRowSpan" => "int",
                "GridColumnSpan" => "int",
                "X" => "int",
                "Y" => "int",
                _ => null
            };
            Match? lateBindRegex = null;
            if (type != null)
            {
                lateBindRegex = Regex.Match(type, @"^([\D\S].*)\*$");
                if (lateBindRegex.Success)
                    type = lateBindRegex.Groups[1].ToString();
            }
            needsLateBind = lateBindRegex?.Success ?? false;

            return type;
        }

        public static string ParseHintedString(string hintedString, string root,
            string? overrideType = null, string componentPrefix = "")
        {
            // hinted value pattern ([name]="[type]: [value]")
            var regex = Regex.Match(hintedString, @"^(\w+):\s?(.+)$");
            var value = hintedString;

            // attribute value-part has datatype hint
            if (!regex.Success && overrideType is null) return value;
            var type = overrideType ?? regex.Groups[1].ToString().ToLower();
            var raw = overrideType is null ? regex.Groups[2].ToString() : hintedString;
            string? specificComponent = null;
            string? enumFamily = null;
            var componentRegex = Regex.Match(type, @"^component<(\D[\w\d]*)>$");
            var enumRegex = Regex.Match(type, @"^enum<(\D[\w\d]*)>$");
            if (componentRegex.Success)
            {
                specificComponent = componentRegex.Groups[1].ToString();
                type = "component";
            }
            if (enumRegex.Success)
            {
                enumFamily = enumRegex.Groups[1].ToString();
                type = "enum";
            }
            try
            {
                value = type switch
                {
                    "bool" => $"{bool.Parse(raw)}",
                    "byte" => $"{byte.Parse(raw)}",
                    "char" => $"'{(raw[0] == '\'' ? "\'" : raw[0])}'",
                    "short" => $"{short.Parse(raw)}",
                    "ushort" => $"{ushort.Parse(raw)}",
                    "int" => $"{int.Parse(raw)}",
                    "uint" => $"{uint.Parse(raw)}",
                    "long" => $"{long.Parse(raw)}",
                    "ulong" => $"{ulong.Parse(raw)}",
                    "float" => $"{float.Parse(raw)}",
                    "double" => $"{double.Parse(raw)}",
                    "string" => $"\"{EscapeString(raw)}\"",
                    "component" => $"{(specificComponent != null ? $"({specificComponent})" : "")}{ParseNestedComponentName(componentPrefix + raw, root)}",
                    "Cerulean.Common.Color" => $"new Cerulean.Common.Color(\"{raw}\")",
                    "Cerulean.Common.Size" => $"new Cerulean.Common.Size({raw})",
                    "literal" => value,
                    "enum" => $"{enumFamily}.{raw}",
                    _ => "null"
                };
                switch (value)
                {
                    case "null":
                        //ColoredConsole.WriteLine(
                        //    $"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Type '{type}' for attribute '{hintedString}' not recognized, using null instead.");
                        break;
                    case "Colors.None" when type == "color":
                        //ColoredConsole.WriteLine(
                        //    $"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Color '{hintedString}' not parsed correctly, using Colors.None instead.");
                        break;
                }
            }
            catch (Exception)
            {
                //ColoredConsole.WriteLine(
                //    $"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Could not cast type '{type}' for attribute '{hintedString}': {raw}, using null instead. {ex.Message}");
                value = "null";
            }

            return value;
        }

        public static string ParseNestedComponentName(string nestedName, string root)
        {
            StringBuilder component = new();
            var nests = nestedName.Split('.');
            component.Append(root).Append("GetChild(\"").Append(nests[0]).Append("\")");
            for (var i = 1; i < nests.Length; i++) component.Append(".GetChild(\"").Append(nests[i]).Append("\")");
            return component.ToString();
        }

        private static string EscapeString(string sourceString)
        {
            return sourceString
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}
