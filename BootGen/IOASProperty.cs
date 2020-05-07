namespace BootGen
{
    public interface IOASProperty
    {
        string Name { get; set; }
        bool Required { get; set; }
        string Type { get; set; }
        string Format { get; set; }
    }
}