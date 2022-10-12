using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    public class RectangleRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Rectangle";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public RectangleRef()
        {
            var tuples = new[]
            {
                ("Size", "size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("FillColor", "color"),
                ("BorderColor", "color"),
                ("FillOpacity", "double"),
                ("BorderOpacity", "double"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
