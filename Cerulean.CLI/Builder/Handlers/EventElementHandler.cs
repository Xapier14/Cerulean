using Cerulean.Common;
using System.Text;
using System.Xml.Linq;

namespace Cerulean.CLI;

[ElementType("Event")]
internal class EventElementHandler : IElementHandler
{
    public bool EvaluateIntoCode(StringBuilder stringBuilder, int indentDepth, XElement element, IBuilder builder,
        IBuilderContext context, string parent = "")
    {
        var eventName = element.Attribute("Name")?.Value;
        var eventHandler = element.Attribute("Handler")?.Value;
        var parentType = element.Parent?.Name.LocalName;
        var targetComponent = element.Attribute("Component")?.Value;
        var componentType = element.Attribute("Type")?.Value;
        if (eventName is null || eventHandler is null || parentType is null)
            return false;

        return parentType is "Layout"
            ? InterpretAsTopLevelEvent(stringBuilder, indentDepth, eventName, eventHandler, targetComponent,
                componentType)
            : InterpretAsNestedEvent(stringBuilder, indentDepth, parentType, parent, eventName, eventHandler);
    }

    private static bool InterpretAsTopLevelEvent(StringBuilder stringBuilder, int indentDepth, string eventName,
        string eventHandler, string? targetComponent, string? componentType)
    {
        if (targetComponent is null || componentType is null)
            return false;
        var eventString = $"(({componentType})GetChild(\"{targetComponent}\")).{eventName} += {eventHandler};\n";
        stringBuilder.AppendIndented(indentDepth, eventString);
        return true;
    }

    private static bool InterpretAsNestedEvent(StringBuilder stringBuilder, int indentDepth, string parentType,
        string parent, string eventName, string eventHandler)
    {
        var eventString = $"(({parentType}){parent}).{eventName} += {eventHandler};\n";
        stringBuilder.AppendIndented(indentDepth, eventString);
        return true;
    }
}