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

        public static string GetType(TypeDescription property)
        {
            string baseType = GetBaseType(property);
            if (property.IsCollection)
                return $"List<{baseType}>";
            return baseType;
        }

        public static List<string> GetPropertiesToLoad(ClassModel c)
        {
            return GetPropertiesToLoadR(c);

        }

        private static List<string> GetPropertiesToLoadR(ClassModel c, List<ClassModel> parents = null, string prefix = null)
        {
            var result = new List<string>();
            foreach (var property in c.Properties)
            {
                if (property.BuiltInType == BuiltInType.Object && !property.IsParentReference)
                {
                    string newPrefix;
                    if (prefix == null)
                    {
                        newPrefix = property.Name;
                    }
                    else
                    {
                        newPrefix = $"{prefix}.{property.Name}";
                    }
                    result.Add(newPrefix);

                    if (parents?.Contains(c) != true)
                    {
                        var newParents = parents != null ? new List<ClassModel>(parents) : new List<ClassModel>();
                        newParents.Add(c);
                        result.AddRange(GetPropertiesToLoadR(property.Class, newParents, newPrefix));
                    }
                }
            }
            return result;
        }

        public static string GetBaseType(TypeDescription property)
        {
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    return "bool";
                case BuiltInType.Int32:
                    return "int";
                case BuiltInType.Int64:
                    return "long";
                case BuiltInType.Float:
                    return "float";
                case BuiltInType.Double:
                    return "double";
                case BuiltInType.String:
                    return "string";
                case BuiltInType.DateTime:
                    return "DateTime";
                case BuiltInType.Object:
                    return property.Class.Name;
            }
            return "object";
        }

        public static string ControllerName(Resource resource)
        {
            return resource.GenerationSettings.ControllerName ?? $"{FullName(resource)}Controller";
        }
        
        public static string ServiceName(Resource resource)
        {
            return resource.GenerationSettings.ServiceName ?? $"{resource.Class.Name.Plural}Service";
        }

        public static Property FirstReference(ClassModel pivot)
        {
            return pivot.Properties.First(p => p.Class != null);
        }
        public static Property SecondReference(ClassModel pivot)
        {
            return pivot.Properties.Last(p => p.Class != null);
        }

        private static string FullName(Resource resource)
        {
            var builder = new StringBuilder();
            if (resource is NestedResource nestedResource && nestedResource.ParentResource != null)
                builder.Append(nestedResource.ParentResource.Name.Plural);
            builder.Append(resource.Name.Plural);
            return builder.ToString();
        }

        public static string Parameters(Method method)
        {
            if (method.Parameter == null)
                return string.Empty;
            StringBuilder builder = new StringBuilder();
            if (builder.Length != 0)
                builder.Append(", ");
            builder.Append(method.Parameter.BuiltInType == BuiltInType.Object ? "[FromBody]" : "[FromQuery]");
            builder.Append(" ");
            builder.Append(GetType(method.Parameter));
            builder.Append(" ");
            builder.Append(method.Parameter.Name);
            return builder.ToString();
        }



        public static bool IsLazyLoaded(Property property)
        {
            return property.Class != null && property.PropertyType != PropertyType.ServerOnly;
        }

        public static bool HasLazyLoadedProperties(ClassModel c)
        {
            return c.Properties.Any(IsLazyLoaded);
        }
    }



}
