using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Resource
    {
        public string PluralName { get; set; }
        public string SingularName => PluralName.Substring(0, PluralName.Length - 1);
        public ClassModel ClassModel { get; set; }
        public bool Get { get; set; }
        public bool Post { get; set; }
        public bool ItemGet { get; set; }
        public bool ItemPut { get; set; }
        public bool ItemDelete { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public Route PermissionRoute { get; set; }
        public List<Resource> ParentResources { get; set; }
        public Resource ParentResource => ParentResources.LastOrDefault();
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; internal set; }
        public bool UsePermissions { get => ClassModel.UsePermissions; internal set => ClassModel.UsePermissions = value; }
        public bool HasPermissions { get => ClassModel.HasPermissions; internal set => ClassModel.HasPermissions = value; }
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
