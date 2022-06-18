using System.Reflection;

namespace Cerulean.CLI.Extensions
{
    internal static class AssemblyExtensions
    {
        public static Type?[] GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
