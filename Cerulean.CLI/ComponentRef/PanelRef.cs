using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

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
                ("Size", "Cerulean.Common.Size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("BackColor", "Cerulean.Common.Color"),
                ("BorderColor", "Cerulean.Common.Color"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
