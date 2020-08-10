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
        public ClassModel Class { get; set; }
        public bool IsReadonly { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public Route PermissionRoute { get; set; }
        public List<Resource> ParentResources { get; set; }
        public Resource ParentResource => ParentResources.LastOrDefault();
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; internal set; }
        public bool UsePermissions { get => Class.UsePermissions; internal set => Class.UsePermissions = value; }
        public bool HasPermissions { get => Class.HasPermissions; internal set => Class.HasPermissions = value; }
        public bool HasTimestamps { get => Class.HasTimestamps; internal set => Class.HasTimestamps = value; }
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
