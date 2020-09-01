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

        public static string NameSpace { get; set; }

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
            foreach (var res in resource.ParentResources)
                builder.Append(res.PluralName);
            builder.Append(resource.PluralName);
            builder.Append("Controller");
            return builder.ToString();
        }

        public static string GetParameters(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation);
        }
        public static string PutParameters(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Put);
            return Parameters(operation);
        }
        public static string DeleteParameters(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Delete);
            return Parameters(operation);
        }
        public static string PostParameters(Resource resource)
        {
            var operation = resource.Route.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Post);
            return Parameters(operation);
        }
        public static string ItemDeleteParameters(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Delete);
            return Parameters(operation);
        }
        public static string ItemPutParameters(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Put);
            return Parameters(operation);
        }

        public static string ItemGetParameters(Resource resource)
        {
            var operation = resource.ItemRoute.Operations.FirstOrDefault(o => o.Verb == HttpVerb.Get);
            return Parameters(operation);
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

        private static string Parameters(Operation operation)
        {
            StringBuilder builder = new StringBuilder();
            if (operation != null)
            {
                foreach (var param in operation.Parameters)
                {
                    if (builder.Length != 0)
                        builder.Append(", ");
                    builder.Append(GetKind(param));
                    builder.Append(" ");
                    builder.Append(GetType(param));
                    builder.Append(" ");
                    builder.Append(param.Name);
                }
                if (operation.Body != null)
                {
                    if (builder.Length != 0)
                        builder.Append(", ");
                    builder.Append("[FromBody] ");
                    if (operation.BodyIsCollection)
                    {
                        builder.Append("List<");
                        builder.Append(operation.Body.Name);
                        builder.Append("> ");
                        builder.Append(operation.Body.Name.ToLower());
                        builder.Append("s");
                    }
                    else
                    {
                        builder.Append(operation.Body.Name);
                        builder.Append(" ");
                        builder.Append(operation.Body.Name.ToLower());
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
