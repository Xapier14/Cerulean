using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public interface IComponentRef
    {
        public string ComponentName { get; }
        public string Namespace { get; }
        public IReadOnlyList<IPropertyRefEntry> Properties { get; }

        public void AddType(string propertyName, string type);
        public string? GetType(string propertyName, out bool needsLateBind);
    }
}
