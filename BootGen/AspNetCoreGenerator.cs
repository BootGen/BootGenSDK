using System.Collections.Generic;
using System.Linq;
using System.Text;
using BootGen;

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
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "bool";
                case BuiltInType.Int:
                    return "int";
                case BuiltInType.Float:
                    return "float";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "DateTime";
                case BuiltInType.Object:
                    return property.Class.Name;
            }
            return "object";
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
