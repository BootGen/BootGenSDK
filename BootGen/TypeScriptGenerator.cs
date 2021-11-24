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
    }
}
