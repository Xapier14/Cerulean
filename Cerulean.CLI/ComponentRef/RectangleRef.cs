using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

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
                ("Size", "Cerulean.Common.Size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("FillColor", "Cerulean.Common.Color"),
                ("BorderColor", "Cerulean.Common.Color"),
                ("FillOpacity", "double"),
                ("BorderOpacity", "double"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
