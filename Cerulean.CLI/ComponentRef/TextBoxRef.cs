using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    public class TextBoxRef : IComponentRef
    {
        public string ComponentName { get; init; } = "TextBox";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public TextBoxRef()
        {
            var tuples = new[]
            {
                ("BackColor", "color"),
                ("BorderColor", "color"),
                ("FocusedColor", "color"),
                ("ForeColor", "color"),
                ("MaxLength", "int"),
                ("Text", "string"),
                ("FontName", "string"),
                ("FontSize", "int"),
                ("FontStyle", "string"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}