using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    public class ImageRef : IComponentRef
    {
        public string ComponentName { get; init; } = "Image";
        public string Namespace { get; init; } = "Cerulean.Components";
        public IEnumerable<PropertyRefEntry> Properties { get; init; }

        public ImageRef()
        {
            var tuples = new[]
            {
                ("Size", "size"),
                ("HintW", "int"),
                ("HintH", "int"),
                ("ImageSource", "string"),
                ("BackColor", "color"),
                ("BorderColor", "color"),
                ("PictureMode", "enum<PictureMode>"),
                ("Opacity", "double"),
                ("Visible", "bool"),
            };
            Properties = PropertyRefEntry.GenerateEntriesFromTuples(tuples);
        }
    }
}
