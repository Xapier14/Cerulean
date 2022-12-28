using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class InputContextRef : IComponentRef
    {
        public string ComponentName { get; init; } = "InputContext";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public InputContextRef()
        {
            var tuples = new[]
            {
                ("SubmitButton", "component<Cerulean.Common.Button>*"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}