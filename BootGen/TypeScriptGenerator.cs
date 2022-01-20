using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootGen
{
    public class TypeScriptGenerator : GeneratorBase
    {
        public TypeScriptGenerator(IDisk disk) : base(disk)
        {
        }

        public static string GetType(Property property)
        {
            return ToTypeScriptType(property.BuiltInType);
        }

        public static string ToTypeScriptType(BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.Bool:
                    return "boolean";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "Date";
                case BuiltInType.Object:
                    return "object";
                default:
                    return "number";
            }
        }

        public static string GetDefaultValue(Property property)
        {
            if (property.IsCollection)
                return "[]";
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "false";
                case BuiltInType.String:
                    return "''";
                case BuiltInType.DateTime:
                    return "new Date()";
                case BuiltInType.Object:
                    return "{}";
                default:
                    return "0";
            }
        }
    }
}
