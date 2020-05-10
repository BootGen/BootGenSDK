using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Operation
    {
        public Operation(HttpMethod method)
        { 
            switch(method)
            {
                case BootGen.HttpMethod.Get:
                Method = "get";
                break;
                case BootGen.HttpMethod.Post:
                Method = "post";
                break;
                case BootGen.HttpMethod.Put:
                Method = "put";
                break;
                case BootGen.HttpMethod.Patch:
                Method = "patch";
                break;
                case BootGen.HttpMethod.Delete:
                Method = "delete";
                break;
            }
        }
        public string Method { get; }
        public string Name { get; internal set; }
        public string Summary { get; internal set; }
        public string Body { get; internal set; }
        public bool BodyIsCollection { get; internal set; }
        public string Response { get; internal set; }
        public bool ResponseIsCollection { get; internal set; }
        public int SuccessCode { get; internal set; }
        public string SuccessDescription { get; internal set; }
        public List<Parameter> Parameters { get; internal set; } = new List<Parameter>();
        public bool HasParameters => Parameters.Any();
    }
}
