namespace Cerulean.Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces all characters in the input string with a new character if any matches with the characters in the character array.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="oldChars">The characters to replace.</param>
        /// <param name="newChar">The character to replace with.</param>
        /// <returns></returns>
        public static string Replace(this string str, char[] oldChars, char newChar)
        {
            return oldChars.Aggregate(str, (current, c) => current.Replace(c.ToString(), newChar == '\0' ? "" : newChar.ToString()));
        }
    }
}