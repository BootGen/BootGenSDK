using System.Collections.Generic;

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
