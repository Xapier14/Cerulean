using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

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
                ("Size", "Cerulean.Common.Size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("BackColor", "Cerulean.Common.Color"),
                ("ForeColor", "Cerulean.Common.Color"),
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
