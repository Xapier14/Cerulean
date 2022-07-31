using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI
{
    /// <summary>
    /// A general component parser.
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
                    attribute.Name.LocalName is not ("Name" or "Data")
            );
            var elementAttributes = element.Attributes().Where(
                attribute => attribute.Name.NamespaceName == "Attribute"
            );
            var ctrData = constructorParams != string.Empty ? $"({constructorParams})" : "";

            // get new object's properties
            var properties =
                from attribute in elementProperties
                let propName = attribute.Name.LocalName
                let propValue = attribute.Value
                select (propName, propValue);
            var formattedProps = properties.Select(prop =>
            {
                var propName = prop.propName;
                var propValue = prop.propValue;
                var recommendedDataType = Helper.GetRecommendedDataType(propName, out var enumFamily);
                var finalPropValue = Helper.ParseHintedString(propValue, parent, enumFamily, recommendedDataType);
                return $"{propName} = {finalPropValue},";
            });

            // write component
            var header = $"{parentPrefix}AddChild(\"{elementName}\", new {elementType}{ctrData} {{\n";
            stringBuilder.AppendIndented(indentDepth, header);

            foreach (var formattedProp in formattedProps)
                stringBuilder.AppendIndented(indentDepth + 1, formattedProp + "\n");

            const string footer = $"}});\n";
            stringBuilder.AppendIndented(indentDepth, footer);

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

            return true;
        }
    }
}
