namespace BootGen
{
    public class Parameter : IOASProperty
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public bool Required { get; set; }
        public string Reference { get; set; }
        public bool IsCollection { get; set; }
    }
}
