using System.Collections.Generic;
using System.Linq;
using System.Text;
using BootGen;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban.Runtime;

namespace BootGen
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
                case BuiltInType.DateTime:
                    return "Date";
                case BuiltInType.Guid:
                    return "string";
                case BuiltInType.Enum:
                    return property.Enum.Name;
                case BuiltInType.Object:
                    return property.Class.Name;
            }
            return "object";
        }

        public static List<string> ReferredClasses(ClassModel c)
        {
            return c.Properties.Where(p => p.Class != null && p.Location != Location.ServerOnly).Select(p => p.Class.Name).Distinct().Concat(c.Properties.Where(p => p.Enum != null).Select(p => p.Enum.Name).Distinct()).ToList();
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
                    builder.Append(resource.ParentResource.Class.Name.ToLower());
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

        public static string ItemPathTemplate(Resource resource)
        {
            var builder = new StringBuilder();
            builder.Append("`");
            foreach (var component in resource.ItemRoute.PathModel)
            {
                if (builder.Length != 1)
                {
                    builder.Append("/");
                }
                if (component.IsVariable)
                {
                    builder.Append("${");
                    if (component == resource.ItemRoute.PathModel.Last())
                        builder.Append(resource.SingularName.ToLower());
                    else
                        builder.Append(resource.ParentResource.Class.Name.ToLower());
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
        public static string GetItemPathTemplate(Resource resource)
        {
            var builder = new StringBuilder();
            builder.Append("`");
            foreach (var component in resource.ItemRoute.PathModel)
            {
                if (builder.Length != 1)
                {
                    builder.Append("/");
                }
                if (component.IsVariable)
                {
                    builder.Append("${");
                    if (component == resource.ItemRoute.PathModel.Last())
                        builder.Append("uuid");
                    else
                    {
                        builder.Append(resource.ParentResource.Class.Name.ToLower());
                        builder.Append(".id");
                    }
                    builder.Append("}");
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
