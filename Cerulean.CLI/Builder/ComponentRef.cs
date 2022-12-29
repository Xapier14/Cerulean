using Cerulean.Common;

namespace Cerulean.CLI
{
    public class ComponentRef : IComponentRef
    {
        private readonly List<IPropertyRefEntry> _properties = new();
        public string ComponentName { get; init; } = "";
        public string Namespace { get; init; } = "";
        public IReadOnlyList<IPropertyRefEntry> Properties => _properties;

        public void AddType(string propertyName, string type)
        {
            _properties.Add(PropertyRefEntry.CreateEntry(propertyName, type));
        }

        public string? GetType(string propertyName, out bool needsLateBind)
        {
            var type = Properties.FirstOrDefault(entry => entry?.PropertyName == propertyName);
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
