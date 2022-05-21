using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace BootGen.Core;

public class DiskTemplateLoader : ITemplateLoader
{
    IDisk Disk { get; }

    public DiskTemplateLoader(IDisk disk)
    {
        Disk = disk;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return templateName;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return Disk.GetFileContent("templatePath");
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return ValueTask.FromResult(Load(context, callerSpan, templatePath));
    }
}

