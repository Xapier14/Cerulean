using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class ButtonRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Button";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public ButtonRef()
        {
            var tuples = new[]
            {
                ("Size", "Cerulean.Common.Size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("Text", "string"),
                ("FontName", "string"),
                ("FontSize", "int"),
                ("FontStyle", "Cerulean.Common.Color"),
                ("BackColor", "Cerulean.Common.Color"),
                ("ForeColor", "Cerulean.Common.Color"),
                ("BorderColor", "Cerulean.Common.Color"),
                ("HighlightColor", "Cerulean.Common.Color"),
                ("ActivatedColor", "Cerulean.Common.Color"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}