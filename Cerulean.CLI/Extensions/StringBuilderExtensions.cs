using System.Text;

namespace Cerulean.CLI;

public static class StringBuilderExtensions
{
    public static void AppendIndented(this StringBuilder stringBuilder, int indent, string text)
    {
        string tabs = new(' ', indent * 4);
        stringBuilder.Append(tabs + text);
    }
}