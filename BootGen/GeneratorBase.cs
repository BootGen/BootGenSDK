using System;
using System.Collections.Generic;
using System.IO;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace BootGen
{
    public class GeneratorBase : ScriptObject
    {
        public string NameSpace { get; set; }
        public IDisk Disk { get; }
        public string TemplateRoot { get; set; }

        public GeneratorBase(IDisk disk)
        {
            Disk = disk;
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
            string content = Render(templateFile, parameters);
            if (!string.IsNullOrEmpty(content))
                Disk.WriteText(folderName, targetFileName, content);
        }

        public string Render(string templateFile, Dictionary<string, object> parameters)
        {
            var template = Parse(templateFile);
            if (template == null) {
                Console.WriteLine($"File not found: {templateFile}");
                return null;
            }
            var context = new TemplateContext();
            context.PushGlobal(this);
            foreach (var param in parameters)
                context.SetValue(new ScriptVariableGlobal(param.Key), param.Value);
            return template.Render(context);
        }

        public void RenderApi(string folderName, string targetFileName, string templateFile, string projectTitle, Api api)
        {
            var template = Parse(templateFile);
            if (template == null) {
                Console.WriteLine($"File not found: {templateFile}");
                return;
            }
            var context = new TemplateContext();
            context.PushGlobal(this);
            context.SetValue(new ScriptVariableGlobal("api"), api);
            context.SetValue(new ScriptVariableGlobal("project_title"), projectTitle);
            var rendered = template.Render(context);
            Disk.WriteText(folderName, targetFileName, rendered);
        }


        public void RenderClasses(string folderName, Func<ClassModel, string> targetFileName, string templateFile, List<ClassModel> classes)
        {
            var template = Parse(templateFile);
            if (template == null) {
                Console.WriteLine($"File not found: {templateFile}");
                return;
            }
            if (template == null) {
                Console.WriteLine($"File not found: {templateFile}");
                return;
            }
            foreach (var c in classes)
            {
                Disk.WriteText(folderName, targetFileName(c), RenderClass(template, c));
            }
        }

        private string RenderClass(Template template, ClassModel c)
        {
            var context = new TemplateContext();
            context.PushGlobal(this);
            context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
            context.SetValue(new ScriptVariableGlobal("class"), c);
            return template.Render(context);;
        }

        public void RenderResources(string folderName, Func<Resource, string> targetFileName, string templateFile, IEnumerable<Resource> resources)
        {
            var template = Parse(templateFile);
            if (template == null) {
                Console.WriteLine($"File not found: {templateFile}");
                return;
            }
            foreach (var resource in resources)
            {
                var context = new TemplateContext();
                context.PushGlobal(this);
                context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
                context.SetValue(new ScriptVariableGlobal("resource"), resource);
                var renderedController = template.Render(context);
                Disk.WriteText(folderName, targetFileName(resource), renderedController);
            }
        }

        private Template Parse(string templateFile)
        {
            string path = templateFile;
            if (!string.IsNullOrWhiteSpace(TemplateRoot))
                path = System.IO.Path.Combine(TemplateRoot, templateFile);
            if (!File.Exists(path))
                return null;
            return Template.Parse(File.ReadAllText(path), templateFile);
        }

    }



}
