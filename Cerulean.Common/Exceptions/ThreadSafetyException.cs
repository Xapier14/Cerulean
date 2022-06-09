namespace Cerulean.Common
{
    public class ThreadSafetyException : Exception
    {
        public ThreadSafetyException() : base()
        {
        }

        public ThreadSafetyException(string message) : base(message)
        {
        }
    }
}