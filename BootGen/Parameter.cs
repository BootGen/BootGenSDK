namespace BootGen
{
    public class Parameter : TypeDescription
    {
        public string Name { get; set; }
        public RestParamterKind Kind { get; set; }
    }

    public enum RestParamterKind { Path, Query, Body }
}
