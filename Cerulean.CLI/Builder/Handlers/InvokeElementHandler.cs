﻿using Cerulean.Common;
using System.Text;
using System.Xml.Linq;

namespace Cerulean.CLI;

[ElementType("Invoke")]
internal class InvokeElementHandler : IElementHandler
{
    public bool EvaluateIntoCode(StringBuilder stringBuilder, int indentDepth, XElement element, IBuilder builder,
        IBuilderContext context, string parent = "")
    {
        var method = element.Attribute("Method")?.Value;
        var args = element.Attribute("Args")?.Value ?? string.Empty;
        args = TypeHelper.ParseHintedString(args, parent);
        var parentType = element.Parent?.Name.LocalName;
        var targetComponent = element.Attribute("Target")?.Value;
        var componentType = element.Attribute("Type")?.Value;
        if (method is null || parentType is null)
            return false;

        return parentType is "Layout"
            ? InterpretAsTopLevelEvent(stringBuilder, indentDepth, method, args, targetComponent,
                componentType)
            : InterpretAsNestedEvent(stringBuilder, indentDepth, method, args, parent, parentType);
    }

    private static bool InterpretAsTopLevelEvent(StringBuilder stringBuilder, int indentDepth, string method,
        string args, string? targetComponent, string? componentType)
    {
        if (targetComponent is null || componentType is null)
            return false;
        var eventString = $"(({componentType})GetChild(\"{targetComponent}\")).{method}({args});\n";
        stringBuilder.AppendIndented(indentDepth, eventString);
        return true;
    }

    private static bool InterpretAsNestedEvent(StringBuilder stringBuilder, int indentDepth, string method,
        string args, string parent, string parentType)
    {
        var eventString = $"(({parentType}){parent}).{method}({args});\n";
        stringBuilder.AppendIndented(indentDepth, eventString);
        return true;
    }
}