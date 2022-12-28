using System.Text;
using System.Xml.Linq;

namespace Cerulean.Common;

public interface IElementHandler
{
    public bool EvaluateIntoCode(StringBuilder stringBuilder, int indentDepth,
        XElement element, Builder builder, BuilderContext context, string parent = "");
}