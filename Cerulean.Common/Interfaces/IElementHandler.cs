using System.Text;
using System.Xml.Linq;

namespace Cerulean.Common;

public interface IElementHandler
{
    public bool EvaluateIntoCode(StringBuilder stringBuilder, int indentDepth,
        XElement element, IBuilder builder, IBuilderContext context, string parent = "");
}