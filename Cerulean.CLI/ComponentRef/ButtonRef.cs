using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                ("Size", "size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("Text", "string"),
                ("FontName", "string"),
                ("FontSize", "int"),
                ("FontStyle", "string"),
                ("BackColor", "color"),
                ("ForeColor", "color"),
                ("BorderColor", "color"),
                ("HighlightColor", "color"),
                ("ActivatedColor", "color"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}