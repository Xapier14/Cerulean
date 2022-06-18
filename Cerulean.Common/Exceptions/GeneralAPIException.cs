namespace Cerulean.Common
{
    public class GeneralAPIException : Exception
    {
        public GeneralAPIException() : base()
        {
        }

        public GeneralAPIException(string message) : base(message)
        {
        }
    }
}
