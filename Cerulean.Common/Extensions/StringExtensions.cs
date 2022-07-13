using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public static class StringExtensions
    {
        public static string Replace(this string str, char[] oldChars, char newChar)
        {
            return oldChars.Aggregate(str, (current, c) => current.Replace(c.ToString(), newChar == '\0' ? "" : newChar.ToString()));
        }
    }
}
