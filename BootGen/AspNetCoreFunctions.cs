using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BootGen;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace IssueTrackerGenerator
{
    public class LanguageBase : ScriptObject
    {
        public string Folder { get; }

        public LanguageBase(string folder)
        {
            Folder = folder;
        }

        public static string KebabCase(string value)
        {
            return value.ToKebabCase();
        }
        public static string SnakeCase(string value)
        {
            return value.ToSnakeCase();
        }

        public static string LowerCase(string value)
        {
            return value.ToLower();
        }
        public static string CamelCase(string value)
        {
            return value[0].ToString().ToLower() + value.Substring(1);
        }

        public void Render(string folderName, string targetFileName, string templateFile, Dictionary<string, object> parameters)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            var context = new TemplateContext();
            context.PushGlobal(this);
            foreach (var param in parameters)
                context.SetValue(new ScriptVariableGlobal(param.Key), param.Value);
            var rendered = template.Render(context);
            File.WriteAllText(System.IO.Path.Combine(dir, targetFileName), rendered);
        }
        public void RenderApi(string folderName, string targetFileName, string templateFile, BootGenApi api)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            var context = new TemplateContext();
            context.PushGlobal(this);
            context.SetValue(new ScriptVariableGlobal("api"), api);
            var rendered = template.Render(context);
            File.WriteAllText(System.IO.Path.Combine(dir, targetFileName), rendered);
        }


        public void RenderClasses(string folderName, Func<ClassModel, string> targetFileName, string templateFile, List<ClassModel> classes)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            foreach (var c in classes)
            {
                var context = new TemplateContext();
                context.PushGlobal(this);
                context.SetValue(new ScriptVariableGlobal("class"), c);
                var renderedModel = template.Render(context);
                File.WriteAllText(System.IO.Path.Combine(dir, targetFileName(c)), renderedModel);
            }
        }

        public void RenderEnums(string folderName, Func<BootGen.EnumModel, string> targetFileName, string templateFile, List<BootGen.EnumModel> enums)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            foreach (var e in enums)
            {
                var context = new TemplateContext();
                context.PushGlobal(this);
                context.SetValue(new ScriptVariableGlobal("enum"), e);
                var renderedModel = template.Render(context);
                File.WriteAllText(System.IO.Path.Combine(dir, targetFileName(e)), renderedModel);
            }
        }
        private string GetPath(string folderName)
        {
            var path = System.IO.Path.Combine(Folder, folderName);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void RenderControllers(string folderName, Func<Controller, string> targetFileName, string templateFile, List<Controller> controllers)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            foreach (var controller in controllers)
            {
                var context = new TemplateContext();
                context.PushGlobal(this);
                context.SetValue(new ScriptVariableGlobal("controller"), controller);
                var renderedController = template.Render(context);
                File.WriteAllText(System.IO.Path.Combine(dir, targetFileName(controller)), renderedController);
            }
        }

        public void RenderResources(string folderName, Func<Resource, string> targetFileName, string templateFile, List<Resource> resources)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            foreach (var resource in resources)
            {
                var context = new TemplateContext();
                context.PushGlobal(this);
                context.SetValue(new ScriptVariableGlobal("resource"), resource);
                var renderedController = template.Render(context);
                File.WriteAllText(System.IO.Path.Combine(dir, targetFileName(resource)), renderedController);
            }
        }

    }

    public class AspNetCoreFunctions : LanguageBase
    {
        public AspNetCoreFunctions(string folder) : base(folder)
        {
        }

        public static string NameSpace => "IssueTracker";

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
