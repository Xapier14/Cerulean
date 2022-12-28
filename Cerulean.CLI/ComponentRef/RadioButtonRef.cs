using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class RadioButtonRef : IComponentRef
    {
        public string ComponentName { get; init; } = "RadioButton";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public RadioButtonRef()
        {
            var tuples = new[]
            {
                ("ForeColor", "Cerulean.Common.Color"),
                ("SelectedColor", "Cerulean.Common.Color"),
                ("Selected", "bool"),
                ("Text", "string"),
                ("FontName", "string"),
                ("FontSize", "int"),
                ("FontStyle", "string"),
                ("WrapText", "bool"),
                ("InputData", "string"),
                ("InputGroup", "string"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}