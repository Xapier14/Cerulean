using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.Core
{
    public class EmbeddedStyles
    {
        private readonly ConcurrentDictionary<(string, string?), Style> _styles;
        private ILoggingService? _logger;
        internal EmbeddedStyles(ILoggingService? loggingService = null)
        {
            _styles = new ConcurrentDictionary<(string, string?), Style>();
            _logger = loggingService;
        }

        internal void SetLogger(ILoggingService loggingService)
        {
            _logger = loggingService;
        }

        internal void RetrieveStyles()
        {
            var styles = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(item => item.IsSubclassOf(typeof(Style)))
                .ToArray();
            foreach (var styleType in styles)
            {
                var style = styleType.GetConstructor(Array.Empty<Type>())
                    ?.Invoke(Array.Empty<object>());
                if (style is null)
                    continue;
                var scopeAttribute = styleType.GetCustomAttributes<ScopeAttribute>().FirstOrDefault();
                var isLocal = scopeAttribute?.Scope == StyleScope.Local;
                _styles.TryAdd((styleType.Name, scopeAttribute?.LocalScopeId), (Style)style);

                _logger?.Log(
                    $"Loaded {(isLocal ? "local" : "global")} style '{styleType.Name}'{(isLocal ? " from '" + scopeAttribute!.LocalScopeId! + "'" : string.Empty)}.");
            }

            _logger?.Log(_styles.IsEmpty
                ? "No styles loaded."
                : $"Loaded {_styles.Count} style(s).");
        }
        
        public Style? FetchStyle(string name, string? localScopeId = null)
        {
            foreach (var ((styleName, localId), style) in _styles)
            {
                if (styleName != name)
                    continue;
                if (localId is not null && localScopeId != localId)
                    continue;
                return style;
            }

            return null;
        }
    }
}
