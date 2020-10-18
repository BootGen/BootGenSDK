

using System;
using System.Linq;
using System.Reflection;

namespace BootGen
{
    internal static class ReflectionUtils
    {
        internal static bool Has<T>(this MemberInfo type) where T : Attribute
        {
            return type.CustomAttributes.Any(d => d.AttributeType == typeof(T));
        }
        internal static CustomAttributeData Get<T>(this MemberInfo type) where T : Attribute
        {
            return type.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(T));
        }
        internal static T GetFirstParameter<T>(this CustomAttributeData attribute)
        {
            return (T)attribute.ConstructorArguments.FirstOrDefault().Value;
        }
    }
}