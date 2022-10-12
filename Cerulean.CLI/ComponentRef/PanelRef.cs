using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    public class PanelRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Panel";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public PanelRef()
        {
            var tuples = new[]
            {
                ("Size", "size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("BackColor", "color"),
                ("BorderColor", "color"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
