using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Operation
    {
        public Operation(Method method)
        { 
            switch(method)
            {
                case BootGen.Method.Get:
                Method = "get";
                break;
                case BootGen.Method.Post:
                Method = "post";
                break;
                case BootGen.Method.Put:
                Method = "put";
                break;
                case BootGen.Method.Patch:
                Method = "patch";
                break;
                case BootGen.Method.Delete:
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
