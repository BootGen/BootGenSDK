namespace BootGen
{
    public interface IOASProperty
    {
        string Name { get; set; }
        bool Required { get; set; }
        string Type { get; set; }
        string Format { get; set; }
        string Reference { get; set; }
        bool IsCollection { get; set; }
    }
}