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
                case BuiltInType.Float:
                    return "number";
                case BuiltInType.Double:
                    return "number";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "Date";
                case BuiltInType.Object:
                    return property.Class.Name;
            }
            return "object";
        }

        public static List<string> ReferredClasses(ClassModel c)
        {
            return c.Properties.Where(p => p.Class != null && p.PropertyType != PropertyType.ServerOnly).Select(p => p.Class.Name.Singular).Distinct().ToList();
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
                if (component.IsVariable && resource is NestedResource nestedResource)
                {
                    builder.Append("${");
                    builder.Append(nestedResource.ParentResource.Name.Singular.ToCamelCase());
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
                        builder.Append(resource.Name.Singular.ToCamelCase());
                    else if (resource is NestedResource nestedResource)
                        builder.Append(nestedResource.ParentResource.Name.Singular.ToCamelCase());
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
                        builder.Append("id");
                    else if (resource is NestedResource nestedResource)
                    {
                        builder.Append(nestedResource.ParentResource.Class.Name.Singular.ToCamelCase());
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

        public static string GetListFunctionName(Resource resource)
        {
            if (resource is NestedResource nestedResource)
                return $"get{resource.Name.Plural}Of{nestedResource.ParentResource.Name}";
            return $"get{resource.Name.Plural}";
        }
        public static string GetItemFunctionName(Resource resource)
        {
            if (resource is NestedResource nestedResource)
                return $"get{resource.Name}Of{nestedResource.ParentResource.Name}";
            return $"get{resource.Name}";
        }
        public static string AddFunctionName(Resource resource)
        {
            if (resource is NestedResource nestedResource)
                return $"add{resource.Name}To{nestedResource.ParentResource.Name}";
            return $"add{resource.Name}";
        }
        public static string UpdateFunctionName(Resource resource)
        {
            if (resource is NestedResource nestedResource)
                return $"update{resource.Name}Of{nestedResource.ParentResource.Name}";
            return $"update{resource.Name}";
        }
        public static string DeleteFunctionName(Resource resource)
        {
            if (resource is NestedResource nestedResource)
                return $"delete{resource.Name}Of{nestedResource.ParentResource.Name}";
            return $"delete{resource.Name}";
        }
    }
}
