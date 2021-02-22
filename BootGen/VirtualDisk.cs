using System.Collections.Generic;

namespace BootGen
{
    public class VirtualDisk : IDisk
    {
        public Dictionary<string, string> Files = new Dictionary<string, string>();
        public void Delete(params string[] path)
        {
        }

        public void WriteText(string folder, string fileName, string content)
        {
            if (string.IsNullOrWhiteSpace(folder))
                Files.Add(fileName, content);
            else
                Files.Add($"{folder}/{fileName}", content);
        }
    }
}
