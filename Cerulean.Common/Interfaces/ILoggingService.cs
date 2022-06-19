namespace Cerulean.Common
{
    public enum LogSeverity
    {
        General,
        Warning,
        Error,
        Fatal
    }
    public interface ILoggingService
    {
        public LogSeverity LoggingLevel { get; set; }
        protected void Init();

        public void Log(string message, LogSeverity severity = LogSeverity.General);

        public void Log(string message, LogSeverity severity, Exception exception);
    }
}