using System.Text;

namespace Cerulean.CLI
{
    internal static class StringBuilderExtensions
    {
        public static void AppendIndented(this StringBuilder stringBuilder, int indent, string text)
        {
            string tabs = new('\t', indent);
            stringBuilder.Append(tabs + text);
        }
    }
}
