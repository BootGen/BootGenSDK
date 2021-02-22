using System.IO;

namespace BootGen
{
    public class Disk : IDisk
    {
        public string Folder { get; }

        public Disk(string folder)
        {
            Folder = folder;
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

        public void WriteText(string folder, string fileName, string content)
        {
            var dir = GetPath(folder);
            File.WriteAllText(System.IO.Path.Combine(dir, fileName), content);
        }

    }



}
