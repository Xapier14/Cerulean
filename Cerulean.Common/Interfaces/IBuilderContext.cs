using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public interface IBuilderContext
    {
        public IList<string> Imports { get; }
        public IList<string> ImportedSheets { get; }
        public IDictionary<string, string> Aliases { get; }
        public IList<(string, string?)> ApplyAsGlobalStyles { get; }
        public string LocalId { get; set; }
        public bool IsStylesheet { get; set; }

        public void UseDefaultImports();
    }
}
