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
                case BuiltInType.Int32:
                    return "integer";
                case BuiltInType.Int64:
                    return "integer";
                case BuiltInType.Float:
                    return "number";
                case BuiltInType.Double:
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
                case BuiltInType.Int32:
                    return "int32";
                case BuiltInType.Int64:
                    return "int64";
                case BuiltInType.Float:
                    return "float";
                case BuiltInType.Double:
                    return "double";
                case BuiltInType.DateTime:
                    return "date-time";
            }
            return null;
        }
    }
}
