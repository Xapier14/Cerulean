namespace Cerulean.Common
{
    public class ComponentRef
    {
        private readonly List<PropertyRefEntry> _properties = new List<PropertyRefEntry>();
        public string ComponentName { get; init; } = "";
        public string Namespace { get; init; } = "";
        public IReadOnlyList<PropertyRefEntry> Properties => _properties;

        internal void AddType(string propertyName, string type)
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
