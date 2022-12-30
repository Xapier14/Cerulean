using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using Cerulean.Analyzer.Extensions;

namespace Cerulean.Analyzer
{
    [Generator]
    public class ComponentRefGenerator : ISourceGenerator
    {
        private readonly RefXMLBuilder _refBuilder = new();
        private string _component = string.Empty;
        private string _namespace = string.Empty;

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassSyntaxReceiver());
            _refBuilder.XmlPath = @"D:\Cerulean\Cerulean.Components.xml";
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (ClassSyntaxReceiver?)context.SyntaxReceiver;

            if (syntaxReceiver is null)
                return;

            foreach (var cds in syntaxReceiver.ClassDeclaration)
            {
                if (cds == null)
                    return;
                
                Logger.WriteLine("Writing ref for {0}", cds.Identifier.Text);

                var classSymbol = cds.GetDeclaredSymbol(context.Compilation) as ITypeSymbol;

                if (classSymbol?.BaseType?.Name is not "Component" or "Cerulean.Common.Component")
                {
                    Logger.WriteLine("Skipping {0} as it's base type is not Component", cds.Identifier.Text);
                    Logger.WriteLine("{0}'s base type is {1}", cds.Identifier.Text, classSymbol?.BaseType?.Name ?? "none");
                    continue;
                }

                _component = cds.Identifier.Text;
                _namespace = RetrieveNamespace(cds);
            
                _refBuilder.AddComponent(
                    cds.Identifier.Text,
                    RetrieveNamespace(cds),
                    ProcessMembers(cds, context.Compilation)
                );
            }
        }

        public string RetrieveNamespace(ClassDeclarationSyntax classDeclarationSyntax)
        {
            SyntaxNode syntaxNode = classDeclarationSyntax;
            while (syntaxNode is not NamespaceDeclarationSyntax)
            {
                if (syntaxNode.Parent == null)
                    break;
                syntaxNode = syntaxNode.Parent;
            }
            return ((NamespaceDeclarationSyntax)syntaxNode).Name.ToString() ?? "";
        }

        public IDictionary<string, string> ProcessMembers(ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation)
        {
            var result = new Dictionary<string, string>();
            classDeclarationSyntax.Members.ToList().ForEach(mds => ProcessMember(result, mds, compilation));
            return result;
        }

        public void ProcessMember(IDictionary<string, string> properties, MemberDeclarationSyntax mds, Compilation compilation)
        {
            var memberData = mds.GetDeclaredSymbol(compilation);
            if (memberData is IPropertySymbol propertyData)
                PropertyHandler.Write(properties, propertyData, mds.GetAttributes(compilation));
        }
    }

    class ClassSyntaxReceiver : ISyntaxReceiver
    {
        public IList<ClassDeclarationSyntax> ClassDeclaration { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                ClassDeclaration.Add(cds);
            }
        }
    }
}
