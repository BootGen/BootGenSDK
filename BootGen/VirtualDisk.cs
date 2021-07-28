using System.Collections.Generic;
using System.IO;

namespace BootGen
{
    public class VirtualFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
    }
    public class VirtualDisk : IDisk
    {
        public List<VirtualFile> Files = new List<VirtualFile>();

        IEnumerable<VirtualFile> IDisk.Files => Files;

        public string GetFileContent(string path)
        {
            foreach(var file in Files) {
                if (file.Path == Path.GetDirectoryName(path) && file.Name == Path.GetFileName(path))
                    return file.Content;
            }
            return null;
        }

        public void WriteText(string folder, string fileName, string content)
        {
            Files.Add(new VirtualFile
            {
                Name = fileName,
                Path = folder,
                Content = content
            });
        }
    }
}
