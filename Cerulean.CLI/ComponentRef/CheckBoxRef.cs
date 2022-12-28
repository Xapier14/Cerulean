using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class CheckBoxRef : IComponentRef
    {
        public string ComponentName { get; init; } = "CheckBox";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public CheckBoxRef()
        {
            var tuples = new[]
            {
                ("Size", "Cerulean.Common.Size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("ForeColor", "Cerulean.Common.Color"),
                ("Checked", "bool"),
                ("Text", "string"),
                ("FontName", "string"),
                ("FontSize", "int"),
                ("FontStyle", "string"),
                ("WrapText", "bool"),
                ("InputData", "string"),
                ("InputGroup", "string")
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}