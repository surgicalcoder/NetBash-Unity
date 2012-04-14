using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetBash.Helpers
{
    internal static class ReflectionHelpers
    {
        public static IEnumerable<Type> MarkedWith<T>(this IEnumerable<Type> Types, bool Inherit = true) where T : Attribute
        {
            if (Types == null)
                return null;
            else
                return Types.Where(x =>
                                       {
                                           if (x.IsDefined(typeof(T), Inherit))
                                               return !x.IsAbstract;
                                           else
                                               return false;
                                       });
        }

        public static IEnumerable<MethodInfo> MarkedWith<T>(this IEnumerable<MethodInfo> Methods, bool Inherit = true) where T : Attribute
        {
            if (Methods == null)
                return null;
            return Methods.Where(x =>
                                     {
                                         if (x.IsDefined(typeof(T), Inherit))
                                         {
                                             return !x.IsAbstract;
                                         }
                                         return false;
                                     });
        }
    }
}