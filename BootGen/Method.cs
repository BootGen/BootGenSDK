using System.Collections.Generic;

namespace BootGen
{
    public class Method
    {
        public string Name { get; set; }
        public HttpVerb Verb { get; set; }
        public List<Property> Parameters { get; set; }
        public TypeDescription ReturnType { get; set; }
    }
}