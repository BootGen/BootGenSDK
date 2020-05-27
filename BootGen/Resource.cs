using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Resource
    {
        public string Name { get; set; }
        public bool IsCollection { get; set; }
        public Schema Schema { get; set; }
        public bool Get { get; set; } = true;
        public bool Put { get; set; } = true;
        public bool Patch { get; set; } = true;
        public bool Delete { get; set; } = true;
        public Route Route { get; set; }
        public Route ElementRoute { get; set; }
        public List<Resource> ParentResources { get; set; }
        public Resource ParentResource => ParentResources.LastOrDefault();
        public List<Resource> NestedResources { get; set; }
        internal Type SourceType { get; set; }
    }

    public class RecursionException : Exception
    {
        public RecursionException(string message) : base(message)
        {

        }
    }
    public class InvalidResourceException : Exception
    {
        public InvalidResourceException(string message) : base(message)
        {

        }
    }

    public class IllegalNestingException : Exception
    {
        public IllegalNestingException() : base("Parent of a nested resource must also be a resource.")
        {

        }
    }
}
