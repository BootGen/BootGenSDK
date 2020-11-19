﻿using System.Linq;
using BootGen;
using Scriban.Runtime;

namespace BootGen
{
    public class OASGenerator : GeneratorBase
    {
        public OASGenerator(string folder) : base(folder)
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
                case BuiltInType.Guid:
                    return "string";
                case BuiltInType.Enum:
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
                case BuiltInType.Guid:
                    return "uuid";
            }
            return null;
        }

        public static string GetEnum(TypeDescription property)
        {
            return $"[{property.Enum.Values.Select(s => $"'{s}'").Aggregate((s1, s2) => $"{s1}, {s2}")}]";
        }
    }
}
