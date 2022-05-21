using System.Linq;

namespace BootGen
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }
        public static string ToSnakeCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
        public static string ToKebabCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
        }
        public static string ToWords(this string str)
        {
            string tmp = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x.ToString() : x.ToString()));
            return char.ToUpper(tmp[0]) + tmp.Substring(1);
        }
    }
}