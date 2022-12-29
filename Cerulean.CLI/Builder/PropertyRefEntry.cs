using Cerulean.Common;

namespace Cerulean.CLI
{
    public class PropertyRefEntry : IPropertyRefEntry
    {
        public string PropertyName { get; private init; } = string.Empty;
        public string PropertyType { get; private init; } = string.Empty;

        public static PropertyRefEntry CreateEntry(string propertyName, string propertyType)
        {
            var ret = new PropertyRefEntry
            {
                PropertyName = propertyName.Trim(),
                PropertyType = propertyType.Trim()
            };

            return ret;
        }

        public static IEnumerable<PropertyRefEntry> GenerateEntriesFromTuples(IEnumerable<(string, string)> tuples)
        {
            var returnList = new List<PropertyRefEntry>();
            foreach (var (propName, propType) in tuples)
            {
                var refEntry = new PropertyRefEntry()
                {
                    PropertyName = propName,
                    PropertyType = propType
                };
                returnList.Add(refEntry);
            }

            return returnList;
        }
    }
}
