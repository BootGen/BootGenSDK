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
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "boolean";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "Date";
                default:
                    return "number";
            }
        }
    }
}
