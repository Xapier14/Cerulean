using System.Text;
using System.Xml.Linq;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI;

/// <summary>
///     A general component parser.
/// </summary>
[Ignore]
internal class GeneralElementHandler : IElementHandler
{
    public bool EvaluateIntoCode(StringBuilder stringBuilder, int indentDepth, XElement element,
        Builder builder, string parent = "")
    {
        var elementType = element.Name.LocalName;
        var elementName = element.Attribute("Name")?.Value ?? Builder.GenerateAnonymousName();
        var parentPrefix = parent != string.Empty ? parent + "." : string.Empty;

        // component construction
        var constructorParams = element.Attribute("Data")?.Value ?? string.Empty;
        var elementProperties = element.Attributes().Where(
            attribute =>
                attribute.Name.NamespaceName != "Attribute" &&
                attribute.Name.LocalName is not ("Name" or "Data" or "Style")
        );
        var elementAttributes = element.Attributes().Where(
            attribute => attribute.Name.NamespaceName == "Attribute"
        );
        var ctrData = constructorParams != string.Empty ? $"({constructorParams})" : "";
        var styleName = element.Attribute("Style")?.Value;

        // get new object's properties
        var properties =
            from attribute in elementProperties
            let propName = attribute.Name.LocalName
            let propValue = attribute.Value
            select (propName, propValue);

        // get props
        var lateBoundProps = new List<(string, string)>();
        var formattedProps = properties.Select(prop =>
        {
            var propName = prop.propName;
            var propValue = prop.propValue;
            var recommendedDataType = Helper.GetRecommendedDataType(propName, out var enumFamily, out var lateBound);
            var finalPropValue = Helper.ParseHintedString(propValue, parent, enumFamily, recommendedDataType, lateBound ? $"{elementName}." : string.Empty);
            if (!lateBound)
                return $"{propName} = {finalPropValue},";

            lateBoundProps.Add((propName, finalPropValue));
            return string.Empty;
        });

        // write component
        var header = $"{parentPrefix}AddChild(\"{elementName}\", new {elementType}{ctrData} {{\n";
        stringBuilder.AppendIndented(indentDepth, header);

        foreach (var formattedProp in formattedProps)
        {
            if (formattedProp == string.Empty)
                continue;
            stringBuilder.AppendIndented(indentDepth + 1, formattedProp + "\n");
        }

        const string footer = "});\n";
        stringBuilder.AppendIndented(indentDepth, footer);

        // apply style if specified
        if (styleName is not null)
        {
            var queueStyle =
                $"QueueStyle({parentPrefix}GetChild(\"{elementName}\"), styles.FetchStyle(\"{styleName}\"));\n";
            stringBuilder.AppendIndented(indentDepth, queueStyle);
        }

        // write attributes
        foreach (var attribute in elementAttributes)
        {
            var name = attribute.Name.LocalName;
            var value = Helper.ParseHintedString(attribute.Value, parent);
            var attributeString =
                $"(({elementType}){parentPrefix}GetChild(\"{elementName}\").AddOrUpdateAttribute(\"{name}\", \"{value}\"));\n";
            stringBuilder.AppendIndented(indentDepth, attributeString);
        }

        // write child elements
        var children = element.Elements();
        foreach (var child in children)
        {
            var childString = $"{parent}{(parent != string.Empty ? '.' : parent)}GetChild(\"{elementName}\")";
            builder.ProcessXElement(stringBuilder, indentDepth, child, childString);
        }

        // write late bound props
        var propString = $"{parentPrefix}GetChild<{elementType}>(\"{elementName}\")";
        foreach (var (propName, propValue) in lateBoundProps)
        {
            stringBuilder.AppendIndented(indentDepth, $"{propString}.{propName} = {propValue};\n");
        }

        return true;
    }
}