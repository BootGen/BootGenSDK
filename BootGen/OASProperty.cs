namespace BootGen
{
    public class OASProperty : IOASProperty
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
    }
}