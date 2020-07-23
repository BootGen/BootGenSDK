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
                    return property.EnumSchema.Name;
                case BuiltInType.Object:
                    return property.Schema.Name;
            }
            return "object";
        }

        public static List<string> ReferredSchemas(Schema schema)
        {
            return schema.Properties.Where(p => p.Schema != null && p.Location != Location.ServerOnly).Select(p => p.Schema.Name).Distinct().Concat(schema.Properties.Where(p => p.EnumSchema != null).Select(p => p.EnumSchema.Name).Distinct()).ToList();
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
                    builder.Append(resource.ParentResource.Schema.Name.ToLower());
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
