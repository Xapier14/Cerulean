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
        private readonly ConcurrentDictionary<string, Style> _styles;
        private ILoggingService? _logger;
        internal EmbeddedStyles(ILoggingService? loggingService = null)
        {
            _styles = new ConcurrentDictionary<string, Style>();
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
                _styles.TryAdd(styleType.Name, (Style)style);

                _logger?.Log($"Loaded style '{styleType.Name}'.");
            }

            _logger?.Log(_styles.IsEmpty
                ? "No styles loaded."
                : $"Loaded {_styles.Count} style(s).");
        }

        public Style FetchStyle(string name)
        {
            if (!_styles.TryGetValue(name, out var style))
                throw new GeneralAPIException("Style not found.");
            return style;
        }
    }
}
