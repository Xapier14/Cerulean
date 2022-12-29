
namespace Cerulean.CLI
{
    public class Config
    {
        private static Config? _config;
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();

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
    }
}
