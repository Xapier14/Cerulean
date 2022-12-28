using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.CLI.Extensions
{
    internal static class ConfigExtensions
    {
        public static void UseDefaultConfiguration(this Config config)
        {
            config.SetProperty("SDL_BUNDLE_JSON", @"https://raw.githubusercontent.com/Xapier14/Cerulean/main/.json/sdl.json");
            config.SetProperty("SDL_BUNDLE_JSON_FALLBACK", "https://raw.githubusercontent.com/Xapier14/Cerulean/.json/sdl.json");
            config.SetProperty("CRN_UPDATE_JSON", @"https://raw.githubusercontent.com/Xapier14/Cerulean/.json/crn.json");
            config.SetProperty("CRN_UPDATE_JSON_FALLBACK", @"https://raw.githubusercontent.com/Xapier14/Cerulean/main/.json/crn.json");
            config.SetProperty("CERULEAN_UI_GIT", @"https://github.com/Xapier14/Cerulean.git");
            config.SetProperty("DOTNET_DEFAULT_BUILD_CONFIG", "Debug");
            config.SetProperty("SHOW_DEV_LOG", string.Empty);
            config.SetProperty("BUILD_BRANCH", "");
        }
    }
}
