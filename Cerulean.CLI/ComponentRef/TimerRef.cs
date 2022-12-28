using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class TimerRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Timer";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public TimerRef()
        {
            var tuples = new[]
            {
                ("IsRunning", "bool"),
                ("Interval", "int"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
