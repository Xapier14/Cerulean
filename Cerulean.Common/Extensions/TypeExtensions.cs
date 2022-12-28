using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public static class TypeExtensions
    {
        public static bool IsAssignableTo(this Type type, Type toType)
            => toType.IsAssignableFrom(type);
        public static bool IsAssignableTo<T>(this Type type)
            => typeof(T).IsAssignableFrom(type);
    }
}
