namespace Cerulean.Common
{
    public static class BoolExtensions
    {
        public static string ToLowerString(this bool value)
        {
            return value ? "true" : "false";
        }
    }
}
