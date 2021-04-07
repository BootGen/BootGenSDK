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

        public static string GetType(TypeDescription property)
        {
            string baseType = GetBaseType(property);
            if (property.IsCollection)
                baseType += "[]";
            return baseType;
        }

        private static string GetBaseType(TypeDescription property)
        {
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "boolean";
                case BuiltInType.Int:
                    return "number";
                case BuiltInType.Float:
                    return "number";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "Date";
                case BuiltInType.Object:
                    return property.Class.Name;
            }
            return "object";
        }
    }
}
