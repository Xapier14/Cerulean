
using Cerulean.Common;

namespace Cerulean.Core
{
    public sealed class ConsoleLoggingService : ILoggingService
    {
        private readonly object _lock = new();
        public LogSeverity LoggingLevel { get; set; } = LogSeverity.Fatal;

        void ILoggingService.Init()
        {
            
        }

        public void Log(string message, LogSeverity severity = LogSeverity.General)
        {
            if (severity > LoggingLevel) return;
            lock (_lock)
            {
                Console.WriteLine("[{0}] [{1}] {2}",
                    DateTime.Now,
                    severity,
                    message);
            }
        }

        public void Log(string message, LogSeverity severity, Exception exception)
        {
            // TODO: add logging for exception
            Log(message, severity);
        }
    }
}
