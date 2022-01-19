using System.Linq;

namespace BootGen
{

    public class AspNetCoreGenerator : GeneratorBase
    {
        public AspNetCoreGenerator(IDisk disk) : base(disk)
        {
        }

        public static string GetType(Property property)
        {
            string baseType = GetBaseType(property);
            if (property.IsCollection)
                return $"List<{baseType}>";
            return baseType;
        }

        public static string GetBaseType(Property property)
        {

            BuiltInType builtInType = property.BuiltInType;
            if (builtInType == BuiltInType.Object)
                return property.Class.Name;
            return ToCSharpType(builtInType);
        }

        public static string ToCSharpType(BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.Bool:
                    return "bool";
                case BuiltInType.Float:
                    return "float";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "DateTime";
                case BuiltInType.Object:
                    return "object";
                default:
                    return "int";
            }
        }

        public static string GetDefaultValue(Property property)
        {
            if (property.IsCollection)
            return $"new List<{ GetBaseType(property)}>()";
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "false";
                case BuiltInType.String:
                    return "\"\"";
                case BuiltInType.DateTime:
                    return "DateTime.Now";
                case BuiltInType.Object:
                    return $"new {property.Class.Name}()";
                default:
                    return "0";
            }
        }

        public static Property FirstReference(ClassModel pivot)
        {
            return pivot.Properties.First(p => p.Class != null);
        }
        public static Property SecondReference(ClassModel pivot)
        {
            return pivot.Properties.Last(p => p.Class != null);
        }
    }
}
