using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cerulean.Analyzer.Extensions
{
    internal static class CodeAnalysis
    {
        public static IReadOnlyList<AttributeData> GetAttributes(this AttributeListSyntax attributes, Compilation compilation)
        {
            var acceptedTrees = new HashSet<SyntaxTree>();
            foreach (var attribute in attributes.Attributes)
                acceptedTrees.Add(attribute.SyntaxTree);

            var parentSymbol = attributes.Parent!.GetDeclaredSymbol(compilation);
            var parentAttributes = parentSymbol!.GetAttributes();
            var ret = parentAttributes.Where(
                attribute => acceptedTrees.Contains(attribute.ApplicationSyntaxReference!.SyntaxTree)
            ).ToList();

            return ret;
        }
        public static IReadOnlyList<AttributeData> GetAttributes(this MemberDeclarationSyntax memberDeclarationSyntax, Compilation compilation)
        {
            var attributeData = new List<AttributeData>();
            foreach (var attributeList in memberDeclarationSyntax.AttributeLists)
                attributeData.AddRange(attributeList.GetAttributes(compilation));
            return attributeData;
        }

        public static AttributeData GetAttribute(this AttributeSyntax attribute, Compilation compilation)
        {
            var parentSymbol = attribute.Parent!.Parent!.GetDeclaredSymbol(compilation);
            var parentAttributes = parentSymbol!.GetAttributes();
            return parentAttributes.First(
                attributeData => attributeData.ApplicationSyntaxReference!.SyntaxTree == attribute.SyntaxTree
            );
        }

        public static ISymbol? GetDeclaredSymbol(this SyntaxNode node, Compilation compilation)
        {
            var model = compilation.GetSemanticModel(node.SyntaxTree);
            return model.GetDeclaredSymbol(node);
        }

        public static string GetFullName(this ISymbol namespaceSymbol)
        {
            var builder = new StringBuilder();
            do
            {
                builder.Insert(0, "." + namespaceSymbol.Name);
                namespaceSymbol = namespaceSymbol.ContainingNamespace;
            } while (namespaceSymbol is not null);
            builder.Remove(0, 2);
            return builder.ToString();
        }

        public static void AddSource(this GeneratorExecutionContext context, SourceBuilder source)
        {
            var name = $"{source.Namespace}.{source.ClassName}-{Guid.NewGuid()}.cs";
            context.AddSource(name, source.Build());
        }
    }
}
