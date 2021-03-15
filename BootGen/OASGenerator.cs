using System.Linq;
using BootGen;
using Scriban.Runtime;

namespace BootGen
{
    public class OASGenerator : GeneratorBase
    {
        public OASGenerator(IDisk disk) : base(disk)
        {
        }

        public static string GetType(TypeDescription property)
        {
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "boolean";
                case BuiltInType.Int:
                    return "integer";
                case BuiltInType.Float:
                    return "number";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "string";
            }
            return null;
        }

        public static string GetFormat(TypeDescription property)
        {
            switch (property.BuiltInType)
            {
                case BuiltInType.Int:
                    return "int32";
                case BuiltInType.Float:
                    return "float";
                case BuiltInType.DateTime:
                    return "date-time";
            }
            return null;
        }
    }
}
