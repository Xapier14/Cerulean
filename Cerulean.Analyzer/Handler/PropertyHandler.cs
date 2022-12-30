using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cerulean.Analyzer
{
    internal static class PropertyHandler
    {
        public static void Write(IDictionary<string, string> result, IPropertySymbol property, IReadOnlyList<AttributeData> attributes)
        {
            var type = property.Type.ToString();
            var name = property.Name;

            var componentTypeAttribute = attributes.FirstOrDefault(
                a =>
                {
                    Logger.WriteLine("{0}", a.AttributeClass?.Name ?? "n/a");
                    return a.AttributeClass?.Name == "ComponentTypeAttribute";
                });
            var lateBoundAttribute = attributes.FirstOrDefault(
                a =>
                {
                    Logger.WriteLine("{0}", a.AttributeClass?.Name ?? "n/a");
                    return a.AttributeClass?.Name == "LateBoundAttribute";
                });
            var isLateBound = lateBoundAttribute is not null;

            if (type is "Cerulean.Common.Component" or "Cerulean.Common.Component?"
                || componentTypeAttribute is not null)
            {
                var componentType = (string?)componentTypeAttribute?.ConstructorArguments[0].Value ?? "Cerulean.Common.Component";
                Logger.WriteLine("This is the type {0}", componentType);
                isLateBound = (bool?)componentTypeAttribute?.ConstructorArguments[1].Value ?? true;
                type = $"component<{componentType}>";
            }
            result.Add(name, $"{type.Replace("?", "")}{(isLateBound ? "*" : "")}");
        }
    }
}