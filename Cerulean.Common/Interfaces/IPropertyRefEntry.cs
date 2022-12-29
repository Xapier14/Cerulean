using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public interface IPropertyRefEntry
    {
        public string PropertyName { get; }
        public string PropertyType { get; }
    }
}
