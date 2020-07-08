using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BootGen
{
    public class TypeDescription
    {
        public BuiltInType BuiltInType { get; set; }
        public bool IsCollection { get; set; }
        public Schema Schema { get; set; }
        public EnumSchema EnumSchema { get; set; }
    }

    public enum Location { Both, ServerOnly, ClientOnly } 

    public class Property : TypeDescription
    {
        public Schema ParentSchema { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public Location Location { get; set; }
        public bool IsServerOnly => Location == Location.ServerOnly;
        public bool IsClientOnly => Location == Location.ClientOnly;
        public List<string> Tags { get; } = new List<string>();
        public bool HasTag(string tag) => Tags.Contains(tag);
        public Property MirrorProperty { get; set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public enum BuiltInType { String, Int32, Int64, Bool, DateTime, Object, Enum }
}
