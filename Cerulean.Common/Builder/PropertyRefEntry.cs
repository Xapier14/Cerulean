namespace Cerulean.Common
{
    public class PropertyRefEntry
    {
        public string PropertyName { get; private set; } = string.Empty;
        public string PropertyType { get; private set; } = string.Empty;

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
