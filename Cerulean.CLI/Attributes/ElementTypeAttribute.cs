using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ElementTypeAttribute : Attribute
    {
        public string ElementType { get; init; }

        public ElementTypeAttribute(string elementType)
        {
            ElementType = elementType;
        }
    }
}
