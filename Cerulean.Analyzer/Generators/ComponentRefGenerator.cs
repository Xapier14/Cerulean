using System;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Linq;
using Cerulean.Analyzer.Extensions;

namespace Cerulean.Analyzer
{
    [Generator]
    public class ComponentRefGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = (ClassSyntaxReceiver?)context.SyntaxReceiver;

            var cds = syntaxReceiver?.ClassDeclaration;

            if (cds == null)
                return;

            if (cds.Identifier.Text.EndsWith("Ref"))
                return;

            var classAttributes = cds.AttributeLists.FirstOrDefault()?.GetAttributes(context.Compilation);
            Logger.WriteLine("Writing ref for {0}", cds.Identifier.Text);
            var className = cds.Identifier.Text + "Ref";
            var source = new SourceBuilder
            {
                Namespace = RetrieveNamespace(cds),
                ClassName = className
            };
            source.AddUsingDirective("Cerulean.Common");
            source.AddInheritance("IComponentRef");
            source.AddInitProperty("string", "ComponentName", $"\"{cds.Identifier.Text}\"");
            source.AddInitProperty("string", "Namespace", $"\"{RetrieveNamespace(cds)}\"");
            source.AddInitProperty("IEnumerable<PropertyRefEntry>", "Properties");

            Logger.WriteLine("SourceBuilder created.");
            ProcessMembers(source, cds, context.Compilation);
            Logger.WriteLine("Members processed.");
            File.WriteAllText(@"D:\Cerulean\test\class-gen.txt", source.ToString());
            Logger.WriteLine("Wrote test gen file.");
            context.AddSource(source);
        }

        public void Output(GeneratorExecutionContext context, string hintName, string s)
        {
            hintName = hintName.Select(x => char.IsLetterOrDigit(x) ? x : '_').ToArray().AsSpan().ToString();
            var name = $"{hintName}.{Guid.NewGuid()}.cs";
            File.WriteAllText(@"D:\Cerulean\src\Lab\" + name, s);
            context.AddSource(name, s);
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

        public void ProcessMembers(SourceBuilder sourceBuilder, ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation)
        {
            sourceBuilder.WriteLineToConstructor("var tuples = new[]", "{");
            sourceBuilder.IncreaseIndentToConstructor();
            classDeclarationSyntax.Members.ToList().ForEach(mds => ProcessMember(sourceBuilder, mds, compilation));
            sourceBuilder.DecreaseIndentToConstructor();
            sourceBuilder.WriteLineToConstructor("};", "Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);");
        }

        public void ProcessMember(SourceBuilder sourceBuilder, MemberDeclarationSyntax mds, Compilation compilation)
        {
            var memberData = mds.GetDeclaredSymbol(compilation);
            if (memberData is IPropertySymbol propertyData)
                PropertyHandler.Write(sourceBuilder, propertyData, mds.GetAttributes(compilation));
        }
    }

    class ClassSyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax? ClassDeclaration { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                ClassDeclaration = cds;
            }
        }
    }
}
