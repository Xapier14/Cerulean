using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    public class LabelRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Label";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public LabelRef()
        {
            var tuples = new[]
            {
                ("Size", "size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("BackColor", "color"),
                ("ForeColor", "color"),
                ("Text", "string"),
                ("FontName", "string"),
                ("FontSize", "int"),
                ("FontStyle", "string"),
                ("WrapText", "string"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
