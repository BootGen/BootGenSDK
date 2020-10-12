using System.Collections.Generic;

namespace BootGen
{
    public class Method
    {
        public string Name { get; set; }
        public HttpVerb Verb { get; set; }
        public Parameter Parameter { get; set; }
        public TypeDescription ReturnType { get; set; }
    }
}