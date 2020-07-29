using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Operation
    {
        public HttpVerb Verb { get; set; }
        public string Name { get; internal set; }
        public string Summary { get; internal set; }
        public Schema Body { get; internal set; }
        public bool BodyIsCollection { get; internal set; }
        public Schema Response { get; internal set; }
        public bool ResponseIsCollection { get; internal set; }
        public int SuccessCode { get; internal set; }
        public string SuccessDescription { get; internal set; }
        public List<Parameter> Parameters { get; internal set; } = new List<Parameter>();
        public bool HasParameters => Parameters.Any();
    }
}
