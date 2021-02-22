namespace BootGen
{
    public interface IDisk
    {
        void WriteText(string folder, string fileName, string content);
        void Delete(params string[] path);
    }
}
