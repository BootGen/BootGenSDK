using System;
using System.Collections.Generic;

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
    }

    public enum BuiltInType { String, Int32, Int64, Bool, Object }

    public class Route
    {
        public Path Path { get; internal set; }
        public List<Operation> Operations { get; internal set; }
    }

    public class Path : List<PathComponent>
    {

    }

    public class PathComponent
    {
        public bool IsVariable { get; internal set; }
        public string Name { get; internal set; }
    }

    public class Operation
    {
        public Method Method { get; internal set; }
        public string Name { get; internal set; }
    }

    public enum Method { Get, Post, Put, Petch, Delete }
}
