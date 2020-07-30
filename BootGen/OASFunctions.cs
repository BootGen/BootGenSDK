using System.Linq;
using BootGen;
using Scriban.Runtime;

namespace IssueTrackerGenerator
{
    public class OASFunctions : LanguageBase
    {
        public OASFunctions(string folder) : base(folder)
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
                case BuiltInType.String:
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
                case BuiltInType.DateTime:
                    return "date-time";
            }
            return null;
        }

        public static string GetEnum(TypeDescription property)
        {
            return $"[{property.Enum.Values.Select(s => $"'{s}'").Aggregate((s1, s2) => $"{s1}, {s2}")}]";
        }
    }
}
