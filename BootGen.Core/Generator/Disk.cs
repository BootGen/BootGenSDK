using System.Collections.Generic;
using System.IO;

namespace BootGen.Core;
public class Disk : IDisk
{
    public string Folder { get; }
    public IEnumerable<VirtualFile> Files => GetFiles(Folder);

    public Disk(string folder)
    {
        Folder = folder;
    }


    private IEnumerable<VirtualFile> GetFiles(string path)
    {
        foreach(var fileName in Directory.EnumerateFiles(path)) {
            yield return new VirtualFile {
                Path = path,
                Name = Path.GetFileName(fileName),
                Content = File.ReadAllText(fileName)
            };
        }
        foreach(var subPath in Directory.EnumerateDirectories(path))
            foreach (var file in GetFiles(subPath))
                yield return file;
    }

    private string GetPath(string folderName)
    {
        var path = System.IO.Path.Combine(Folder, folderName);
        if (!Directory.Exists(path))
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

    public void Delete(params string[] path)
    {
        var p = Folder;
        foreach (var part in path)
            p = System.IO.Path.Combine(p, part);
        File.Delete(p);
    }

    public string GetFileContent(string path)
    {
        path = Path.Combine(Folder, path);
        if (!File.Exists(path))
            return null;
        return File.ReadAllText(path);
    }
}
