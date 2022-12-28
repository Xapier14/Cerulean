using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class ProgressBarRef : IComponentRef
    {
        public string ComponentName { get; init; } = "ProgressBar";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public ProgressBarRef()
        {
            var tuples = new[]
            {
                ("Size", "Cerulean.Common.Size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("BackColor", "Cerulean.Common.Color"),
                ("BorderColor", "Cerulean.Common.Color"),
                ("Orientation", "enum<Cerulean.Common.Orientation>"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
