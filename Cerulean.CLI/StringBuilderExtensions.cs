using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
