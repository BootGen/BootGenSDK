using System.Collections.Generic;

namespace BootGen.Core;
public interface IDisk
{
    void WriteText(string folder, string fileName, string content);
    IEnumerable<VirtualFile> Files { get; }
    string GetFileContent(string path);
}
