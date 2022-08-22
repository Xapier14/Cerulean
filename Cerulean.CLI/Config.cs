using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI
{
    internal class Config
    {
        private static Config? _config;
        private IDictionary<string, object> _properties = new Dictionary<string, object>();

        private Config() { }

        public static Config GetConfig()
        {
            return _config ??= new Config();
        }

        public object? GetProperty(string key)
        {
            return _properties.ContainsKey(key)
                ? _properties[key]
                : null;
        }

        public T? GetProperty<T>(string key)
        {
            return (T?)GetProperty(key);
        }

        public void SetProperty(string key, object? value)
        {
            if (value == null)
            {
                _properties.Remove(key);
                return;
            }
            _properties[key] = value;
        }

        public void UseDefaultConfiguration()
        {
            SetProperty("SDL_BUNDLE_JSON", @"https://raw.githubusercontent.com/Xapier14/Cerulean/main/.json/sdl.json");
            SetProperty("SDL_BUNDLE_JSON_FALLBACK", "https://raw.githubusercontent.com/Xapier14/Cerulean/.json/sdl.json");
            SetProperty("CRN_UPDATE_JSON", @"https://raw.githubusercontent.com/Xapier14/Cerulean/.json/crn.json");
            SetProperty("CRN_UPDATE_JSON_FALLBACK", @"https://raw.githubusercontent.com/Xapier14/Cerulean/main/.json/crn.json");
            SetProperty("CERULEAN_UI_GIT", @"https://github.com/Xapier14/Cerulean.git");
            SetProperty("DOTNET_DEFAULT_BUILD_CONFIG", "Debug");
        }
    }
}
