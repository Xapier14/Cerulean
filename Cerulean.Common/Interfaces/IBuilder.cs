using System.Text;
using System.Xml.Linq;

namespace Cerulean.Common
{
    public interface IBuilder
    {
        public IReadOnlyDictionary<string, string> ExportedLayouts { get; }
        public IReadOnlyDictionary<string, string> ExportedStyles { get; }
        public IReadOnlyDictionary<string, IBuilderContext> Sheets { get; }
        public IReadOnlyList<IComponentRef> ComponentReferences { get; }

        public bool LexContentFromXml(IBuilderContext context, string xmlFilePath);
        public void Build();
        public bool IsComponentFromNamespace(string componentType, string namespacePart);
        public void ProcessXElement(IBuilderContext context, StringBuilder stringBuilder, int depth, XElement element,
            string parent = "");
        public string GenerateAnonymousName(string? prefix = null);
    }
}
