using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.CLI;

namespace Cerulean.CLI
{
    public interface IComponentRef
    {
        public string ComponentName { get; init; }
        public string Namespace { get; init; }
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public string? GetType(string propertyName, out bool needsLateBind)
        {
            var type = Properties.FirstOrDefault((entry => entry?.PropertyName == propertyName), null);
            needsLateBind = false;
            var propName = type?.PropertyType;

            if (propName is null)
                return propName;

            needsLateBind = propName.EndsWith('*');
            if (needsLateBind)
                propName = propName.Remove(propName.Length - 1, 1);
            return propName;
        }
    }
}
