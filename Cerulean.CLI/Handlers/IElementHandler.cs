using System.Text;
using System.Xml.Linq;

namespace Cerulean.CLI;

public interface IElementHandler
{
    public bool EvaluateIntoCode(StringBuilder stringBuilder, int indentDepth,
        XElement element, Builder builder, string parent = "");
}