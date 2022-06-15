using System.Reflection;
using Cerulean.Common;
using System.Collections.Concurrent;

namespace Cerulean.Core
{
    public class EmbeddedLayouts
    {
        private readonly ConcurrentDictionary<string, ConstructorInfo> _layouts;
        private ILoggingService? _logger;
        internal EmbeddedLayouts(ILoggingService? loggingService = null)
        {
            _layouts = new();
            _logger = loggingService;
        }

        internal void SetLogger(ILoggingService loggingService)
        {
            _logger = loggingService;
        }

        internal void RetrieveLayouts()
        {
            var layouts = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(item => item.IsSubclassOf(typeof(Layout)))
                .ToArray();
            foreach (var layoutType in layouts)
            {
                var layoutConstructor = layoutType.GetConstructor(Array.Empty<Type>());
                if (layoutConstructor is null)
                {
                    _logger?.Log($"Could not get constructor for layout '{layoutType.Name}'.");
                    continue;
                }

                if (!_layouts.TryAdd(layoutType.Name, layoutConstructor))
                {
                    _logger?.Log($"A constructor for layout '{layoutType.Name}' is already registered.");
                    continue;
                }
                
                _logger?.Log($"Loaded layout '{layoutType.Name}'.");
            }
            if (_layouts.IsEmpty)
            {
                _logger?.Log("No layouts loaded.");
            } else
            {
                _logger?.Log($"Loaded {_layouts.Count} layout(s).");
            }
        }

        public Layout FetchLayout(string name)
        {
            if (_layouts.TryGetValue(name, out var layoutConstructor))
                try
                {
                    return (Layout)layoutConstructor.Invoke(Array.Empty<object>());
                }
                catch (TargetInvocationException ex)
                {
                    _logger?.Log($"Ctor error constructing layout '{name}'.");
                    if (ex.InnerException is not null)
                        throw new FatalAPIException(ex.InnerException.Message);
                }
                catch (Exception ex)
                {
                    _logger?.Log($"General error constructing layout '{name}'.");
                    throw new FatalAPIException(ex.Message);
                }
            throw new GeneralAPIException("Layout not found.");
            
        }
    }
}