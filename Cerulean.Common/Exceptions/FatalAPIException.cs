namespace Cerulean.Common
{
    public class FatalAPIException : Exception
    {
        public FatalAPIException() : base()
        {
        }

        public FatalAPIException(string message) : base(message)
        {
        }
    }
}
