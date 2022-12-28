using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public class PropertyRefEntry
    {
        public string PropertyName { get; private init; } = string.Empty;
        public string PropertyType { get; private init; } = string.Empty;

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
