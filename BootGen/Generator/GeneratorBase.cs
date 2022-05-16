﻿using System;
using System.Collections.Generic;
using System.IO;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace BootGen;
public class GeneratorBase : ScriptObject
{
    public string NameSpace { get; set; }
    public IDisk Disk { get; }
    public IDisk Templates { get; set; }

    public GeneratorBase(IDisk disk)
    {
        Disk = disk;
    }

    public static string SnakeCase(string value)
    {
        return value.ToSnakeCase();
    }

    public static string KebabCase(string value)
    {
        return value.ToKebabCase();
    }

    public static string CamelCase(string value)
    {
        return value.ToCamelCase();
    }
    public static string ToWords(string value)
    {
        return value.ToWords();
    }

    public void Render(string folderName, string targetFileName, string templateFile, Dictionary<string, object> parameters)
    {
        string content = Render(templateFile, parameters);
        if (!string.IsNullOrEmpty(content))
            Disk.WriteText(folderName, targetFileName, content);
    }

    public string Render(string templateFile, Dictionary<string, object> parameters)
    {
        var template = LoadTemplate(templateFile);
        if (template == null)
        {
            return null;
        }
        var context = CreateContext();
        context.PushGlobal(this);
        foreach (var param in parameters)
            context.SetValue(new ScriptVariableGlobal(param.Key), param.Value);
        return template.Render(context);
    }

    private TemplateContext CreateContext()
    {
        return new TemplateContext {
            TemplateLoader = new DiskTemplateLoader(Templates)
        };
    }

    Template LoadTemplate(string templateFile) {
        var template = Parse(templateFile);
        if (template == null) {
            Console.WriteLine($"File not found: {templateFile}");
        }
        return template;
    }


    public void RenderClasses(string folderName, Func<ClassModel, string> targetFileName, string templateFile, IEnumerable<ClassModel> classes)
    {
        var template = LoadTemplate(templateFile);
        if (template == null) {
            return;
        }
        foreach (var c in classes)
        {
            Disk.WriteText(folderName, targetFileName(c), RenderClass(template, c));
        }
    }

    private string RenderClass(Template template, ClassModel c)
    {
        var context = CreateContext();
        context.PushGlobal(this);
        context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
        context.SetValue(new ScriptVariableGlobal("class"), c);
        return template.Render(context);;
    }

    public void RenderResources(string folderName, Func<Resource, string> targetFileName, string templateFile, IEnumerable<Resource> resources)
    {
        var template = LoadTemplate(templateFile);
        if (template == null) {
            return;
        }
        foreach (var resource in resources)
        {
            var context = CreateContext();
            context.PushGlobal(this);
            context.SetValue(new ScriptVariableGlobal("name_space"), NameSpace);
            context.SetValue(new ScriptVariableGlobal("resource"), resource);
            var renderedController = template.Render(context);
            Disk.WriteText(folderName, targetFileName(resource), renderedController);
        }
    }

    private Template Parse(string templateFile)
    {
        string content = Templates.GetFileContent(templateFile);
        if (string.IsNullOrWhiteSpace(content))
            return null;
        return Template.Parse(content, templateFile);
    }

}
