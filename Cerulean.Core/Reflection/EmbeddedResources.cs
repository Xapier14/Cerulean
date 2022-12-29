using Cerulean.Common;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace Cerulean.Core
{
    public class EmbeddedResources
    {
        private readonly ConcurrentDictionary<string, Resource> _resources;
        private ILoggingService? _logger;
        internal EmbeddedResources(ILoggingService? loggingService = null)
        {
            _resources = new ConcurrentDictionary<string, Resource>();
            _logger = loggingService;
        }

        internal void SetLogger(ILoggingService loggingService)
        {
            _logger = loggingService;
        }

        internal void RetrieveResources()
        {
            var resources = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(item => item.IsSubclassOf(typeof(Resource)))
                .ToArray();
            foreach (var resourceType in resources)
            {
                var resourceConstructor = resourceType.GetConstructor(Array.Empty<Type>());
                if (resourceConstructor is null)
                {
                    _logger?.Log($"Could not get constructor for resource '{resourceType.Name}'.", LogSeverity.Warning);
                    continue;
                }

                try
                {
                    var resource = resourceConstructor.Invoke(null);
                    if (!_resources.TryAdd(resourceType.Name, (Resource)resource))
                    {
                        _logger?.Log($"A resource named '{resourceType.Name}' already exists.",
                            LogSeverity.Warning);
                        continue;
                    }

                    _logger?.Log($"Loaded resource '{resourceType.Name}'.");
                }
                catch (Exception)
                {
                    _logger?.Log($"Could not construct resource '{resourceType.Name}'.", LogSeverity.Warning);
                }
            }

            _logger?.Log(_resources.IsEmpty
                ? "No resources loaded."
                : $"Loaded {_resources.Count} resources(s).");
        }

        public Resource FetchImage(string name)
        {
            if (_resources.TryGetValue(name, out var resource)
                && resource.Type == ResourceType.Image)
                return resource;
            throw new GeneralAPIException("Image resource not found.");
        }
        public Resource FetchAudio(string name)
        {
            if (_resources.TryGetValue(name, out var resource)
                && resource.Type == ResourceType.Audio)
                return resource;
            throw new GeneralAPIException("Audio resource not found.");
        }
        public Resource FetchText(string name)
        {
            if (_resources.TryGetValue(name, out var resource)
                && resource.Type == ResourceType.Text)
                return resource;
            throw new GeneralAPIException("Text resource not found.");
        }

        internal IntPtr FetchImageAsRWops(string name)
        {
            var resource = FetchImage(name);
            return RWopsFromResource(resource);
        }

        internal IntPtr FetchAudioAsRWops(string name)
        {
            var resource = FetchAudio(name);
            return RWopsFromResource(resource);
        }

        internal IntPtr FetchTextAsRWops(string name)
        {
            var resource = FetchText(name);
            return RWopsFromResource(resource);
        }

        public static IntPtr RWopsFromResource(Resource resource)
        {
            var length = resource.Data.Length;
            var unmanaged = Marshal.AllocHGlobal(length);
            Marshal.Copy(resource.Data, 0, unmanaged, length);
            return SDL_RWFromMem(unmanaged, length);
        }
    }
}
