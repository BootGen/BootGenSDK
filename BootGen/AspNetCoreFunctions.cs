using System.Collections.Generic;
using System.Linq;
using System.Text;
using BootGen;

namespace BootGen
{

    public class AspNetCoreFunctions : LanguageBase
    {
        public AspNetCoreFunctions(string folder) : base(folder)
        {
        }

        public static string GetType(TypeDescription property)
        {
            string baseType = GetBaseType(property);
            if (property.IsCollection)
                return $"List<{baseType}>";
            if (!property.IsRequired && property.BuiltInType != BuiltInType.Object && property.BuiltInType != BuiltInType.String)
                return $"{baseType}?";
            return baseType;
        }
        public static string GetKind(Parameter param)
        {
            switch (param.Kind)
            {
                case RestParamterKind.Path:
                    return "[FromRoute]";
                case RestParamterKind.Query:
                    return "[FromQuery]";
                case RestParamterKind.Body:
                    return "[FromBody]";
            }
            return string.Empty;
        }

        public static List<string> GetPropertiesToLoad(ClassModel c)
        {
            return GetPropertiesToLoadR(c);

        }

        public static string ParentIdName(Resource resource)
        {
            return resource?.ParentRelation?.ParentIdProperty?.Name;
        }

        private static List<string> GetPropertiesToLoadR(ClassModel c, List<ClassModel> parents = null, string prefix = null)
        {
            var result = new List<string>();
            foreach (var property in c.Properties)
            {
                if (property.BuiltInType == BuiltInType.Object && !property.ParentReference && property.IsCollection)
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
                case BuiltInType.String:
                    return "string";
                case BuiltInType.Guid:
                    return "Guid";
                case BuiltInType.DateTime:
                    return "DateTime";
                case BuiltInType.Object:
                    return property.Class.Name;
                case BuiltInType.Enum:
                    return property.Enum.Name;
            }
            return "object";
        }

        public static string ControllerName(Resource resource)
        {
            var builder = new StringBuilder();
            if (resource.ParentResource != null)
                builder.Append(resource.ParentResource.PluralName);
            builder.Append(resource.PluralName);
            builder.Append("Controller");
            return builder.ToString();
        }

        public static string GetParameters(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation, resource);
        }
        public static string ItemGetParameters(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation, resource);
        }
        public static string PostParameters(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Post);
            return Parameters(operation, resource);
        }
        public static string ItemDeleteParameters(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Delete);
            return Parameters(operation, resource);
        }
        public static string ItemPutParameters(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Put);
            return Parameters(operation, resource);
        }

        public static string GetParametersService(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation, resource, false);
        }
        public static string ItemGetParametersService(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation, resource, false);
        }
        public static string PostParametersService(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Post);
            return Parameters(operation, resource, false);
        }
        public static string ItemDeleteParametersService(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Delete);
            return Parameters(operation, resource, false);
        }
        public static string ItemPutParametersService(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Put);
            return Parameters(operation, resource, false);
        }

        public static string GetParametersCall(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation, resource, false, false);
        }
        public static string ItemGetParametersCall(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation, resource, false, false);
        }
        public static string PostParametersCall(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Post);
            return Parameters(operation, resource, false, false);
        }
        public static string ItemDeleteParametersCall(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Delete);
            return Parameters(operation, resource, false, false);
        }
        public static string ItemPutParametersCall(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Put);
            return Parameters(operation, resource, false, false);
        }

        public static string Parameters(Method method)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var param in method.Parameters)
            {
                if (builder.Length != 0)
                    builder.Append(", ");
                builder.Append(param.BuiltInType == BuiltInType.Object ? "[FromBody]" : "[FromQuery]");
                builder.Append(" ");
                builder.Append(GetType(param));
                builder.Append(" ");
                builder.Append(param.Name);
            }
            return builder.ToString();
        }

        private static string Parameters(Operation operation, Resource resource, bool withAttributes = true, bool withTypes = true)
        {
            StringBuilder builder = new StringBuilder();
            if (operation != null)
            {
                foreach (var param in operation.Parameters)
                {
                    if (builder.Length != 0)
                        builder.Append(", ");
                    if (withAttributes)
                    {
                        builder.Append(GetKind(param));
                        builder.Append(" ");
                    }
                    if (withTypes)
                    {
                        builder.Append(GetType(param));
                        builder.Append(" ");
                    }
                    builder.Append(param.Name);
                }
                if (operation.Body != null)
                {
                    if (builder.Length != 0)
                        builder.Append(", ");
                    if (withAttributes) 
                        builder.Append("[FromBody] ");
                    if (operation.BodyIsCollection)
                    {
                        if (withTypes)
                        {
                            builder.Append("List<");
                            builder.Append(operation.Body.Name);
                            builder.Append("> ");
                        }
                        builder.Append(resource.PluralName.ToCamelCase());
                    }
                    else
                    {
                        if (withTypes)
                        {
                            builder.Append(operation.Body.Name);
                            builder.Append(" ");
                        }
                        builder.Append(resource.Name.ToCamelCase());
                    }
                }
            }
            return builder.ToString();
        }

        public static string ItemRelativePath(Resource resource)
        {
            int count = resource.ItemRoute.PathModel.Count - resource.Route.PathModel.Count;
            var path = new BootGen.Path();
            path.AddRange(resource.ItemRoute.PathModel.TakeLast(count));
            return path.ToString().Substring(1);
        }

        public static bool IsLazyLoaded(Property property)
        {
            return property.Class != null && property.Location != Location.ServerOnly;
        }

        public static bool HasLazyLoadedProperties(ClassModel c)
        {
            return c.Properties.Any(IsLazyLoaded);
        }
    }



}
