using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI
{
    public class GridRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Grid";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public GridRef()
        {
            var tuples = new[]
            {
                ("ColumnCount", "int"),
                ("RowCount", "int"),
                ("BackColor", "Cerulean.Common.Color"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
