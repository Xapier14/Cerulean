using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    public class BuilderContextNew
    {
        public IDictionary<string, string> Layouts { get; init; }
        public IDictionary<string, string> Styles { get; init; }
        public IList<string> Imports { get; init; }
        public IDictionary<string, string> Aliases { get; init; }

        public BuilderContextNew()
        {
            Layouts = new Dictionary<string, string>();
            Styles = new Dictionary<string, string>();
            Imports = new List<string>();
            Aliases = new Dictionary<string, string>();
        }

        public void UseDefaultImports()
        {
            // Namespaces
            Imports.Clear();
            Imports.Add("Cerulean");
            Imports.Add("Cerulean.Core");
            Imports.Add("Cerulean.Common");
            Imports.Add("Cerulean.Components");
        }
    }
    public class BuilderNew
    {

    }
}
