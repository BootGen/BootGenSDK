using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BootGen
{
    public class Property
    {
        public string Name { get; internal set; }
        public BuiltInType Type { get; internal set; }
        public bool IsCollection { get; internal set; }
        public Schema Schema { get; internal set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public class Schema
    {
        public string Name { get; internal set; }
        public List<Property> Properties { get; internal set; }
        public bool IsResource { get; internal set; }
    }

    public enum BuiltInType { String, Int32, Int64, Bool, Object }

    public class Route
    {
        public string Path { get; internal set; }
        public List<Operation> Operations { get; internal set; }
    }

    public class Path : List<PathComponent>
    {
        public string Relative => this.ToString();
        public Path()
        {
        }

        public Path(Path path) : base(path)
        {
        }

        internal Path Adding(PathComponent pathComponent)
        {
            Path path = new Path(this);
            path.Add(pathComponent);
            return path;
        }

        public override string ToString(){
            StringBuilder builder = new StringBuilder();
            foreach (var item in this) {
                builder.Append("/");
                builder.Append(item.ToString());
            }
            return builder.ToString();
        }
    }

    public class PathComponent
    {
        public bool IsVariable { get; internal set; }
        public string Name { get; internal set; }

        public override string ToString(){
            if (IsVariable) {
                return "{" + Name + "}";
            } else {
                return Name;
            }
        }
    }

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
    }

    public enum Method {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }
}
