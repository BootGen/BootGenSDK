using System.Collections.Generic;
using System.Linq;
using System.Text;
using BootGen;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban.Runtime;

namespace IssueTrackerGenerator
{
    public class TypeScriptFunctions : LanguageBase
    {
        public TypeScriptFunctions(string folder) : base(folder)
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
                    return "Boolean";
                case BuiltInType.Int32:
                    return "number";
                case BuiltInType.Int64:
                    return "number";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.Enum:
                    return property.EnumModel.Name;
                case BuiltInType.Object:
                    return property.ClassModel.Name;
            }
            return "object";
        }

        public static List<string> ReferredClasses(ClassModel c)
        {
            return c.Properties.Where(p => p.ClassModel != null && p.Location != Location.ServerOnly).Select(p => p.ClassModel.Name).Distinct().Concat(c.Properties.Where(p => p.EnumModel != null).Select(p => p.EnumModel.Name).Distinct()).ToList();
        }

        public static string PathTemplate(Resource resource)
        {
            var builder = new StringBuilder();
            builder.Append("`");
            foreach (var component in resource.Route.PathModel)
            {
                if (builder.Length != 1)
                {
                    builder.Append("/");
                }
                if (component.IsVariable)
                {
                    builder.Append("${");
                    builder.Append(resource.ParentResource.ClassModel.Name.ToLower());
                    builder.Append(".id}");
                }
                else
                {
                    builder.Append(component.Name);
                }
            }
            builder.Append("`");
            return builder.ToString();
        }
    }
}
