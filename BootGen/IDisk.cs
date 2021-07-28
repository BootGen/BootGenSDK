using System.Collections.Generic;

namespace BootGen
{
    public interface IDisk
    {
        void WriteText(string folder, string fileName, string content);
        IEnumerable<VirtualFile> Files { get; }
        string GetFileContent(string path);
    }
}
