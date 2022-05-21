using System;
using System.Collections.Generic;
using Scriban.Syntax;

namespace BootGen
{
    public static class GeneratorExtensions {


        public static  void RenderResources(this GeneratorBase generator,  string folderName, Func<Resource, string> targetFileName, string templateFile, IEnumerable<Resource> resources)
        {
            var template = generator.LoadTemplate(templateFile);
            if (template == null) {
                return;
            }
            foreach (var resource in resources)
            {
                var context = generator.CreateContext();
                context.PushGlobal(generator);
                context.SetValue(new ScriptVariableGlobal("name_space"), generator.NameSpace);
                context.SetValue(new ScriptVariableGlobal("resource"), resource);
                var renderedController = template.Render(context);
                generator.Disk.WriteText(folderName, targetFileName(resource), renderedController);
            }
        }
    }

}
