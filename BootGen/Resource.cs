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
        public bool Get { get; set; }
        public bool Post { get; set; }
        public bool Delete { get; set; }
        public bool ItemGet { get; set; }
        public bool ItemPost { get; set; }
        public bool ItemDelete { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public List<Resource> ParentResources { get; set; }
        public Resource ParentResource => ParentResources.LastOrDefault();
        public List<Resource> NestedResources { get; set; }
        public Schema Pivot { get; internal set; }
        internal Type SourceType { get; set; }
        public bool UsePermissons { get; set; }
        public Permission GetPermission { get; set; } = Permission.Read;
        public Permission PostPermission { get; set; } = Permission.Write;
        public Permission DeletePermission { get; set; } = Permission.Write;
        public Permission ItemGetPermission { get; set; } = Permission.Read;
        public Permission ItemPostPermission { get; set; } = Permission.Write;
        public Permission ItemDeletePermission { get; set; } = Permission.Write;
    }

    public class InvalidResourceException : Exception
    {
        public InvalidResourceException(string message) : base(message)
        {

        }
    }

    public enum Permission
    {
        None,
        Read,
        Write,
        Own
    }

}
