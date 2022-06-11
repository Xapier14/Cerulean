namespace Cerulean.Common
{
    public interface ILoggingService
    {
        protected void Init();

        public void Log(string message);

        public void Log(string message, Exception exception);
    }
}