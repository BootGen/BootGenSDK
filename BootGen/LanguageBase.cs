using System;
using System.Collections.Generic;
using System.IO;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace BootGen
{
    public class LanguageBase : ScriptObject
    {
        public string Folder { get; }
        public string NameSpace { get; set; }
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
        public void RenderApi(string folderName, string targetFileName, string templateFile, string baseURL, BootGenApi api)
        {
            var dir = GetPath(folderName);
            var template = Template.Parse(File.ReadAllText(templateFile), templateFile);
            var context = new TemplateContext();
            context.PushGlobal(this);
            context.SetValue(new ScriptVariableGlobal("api"), api);
            context.SetValue(new ScriptVariableGlobal("base_url"), baseURL);
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
                context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
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
                context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
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
                context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
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
                context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
                context.SetValue(new ScriptVariableGlobal("resource"), resource);
                var renderedController = template.Render(context);
                File.WriteAllText(System.IO.Path.Combine(dir, targetFileName(resource)), renderedController);
            }
        }

    }



}
