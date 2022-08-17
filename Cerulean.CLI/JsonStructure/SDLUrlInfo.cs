using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.JsonStructure
{
    internal class SDLUrlInfo
    {
        public string? PreferredSDL { get; set; }
        public string? PreferredImage { get; set; }
        public string? PreferredTTF { get; set; }

        public string? MinimumSDL { get; set; }
        public string? MinimumImage { get; set; }
        public string? MinimumTTF { get; set; }
        
        public Dictionary<string, ArchedLinkEntry>? SDL { get; set; }
        public Dictionary<string, ArchedLinkEntry>? Image { get; set; }
        public Dictionary<string, ArchedLinkEntry>? TTF { get; set; }

        internal void Deconstruct(out string preferredSDL, out string preferredImage, out string preferredTTF)
        {
            preferredSDL = PreferredSDL;
            preferredImage = PreferredImage;
            preferredTTF = PreferredTTF;
        }
    }
}
