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
        public bool IsRequired { get; set; }
        public ClassModel ClassModel { get; set; }
        public EnumModel EnumModel { get; set; }
    }

    public enum Location { Both, ServerOnly, ClientOnly }

    public class Property : TypeDescription
    {
        public string Name { get; set; }
        public Location Location { get; set; }
        public bool IsServerOnly => Location == Location.ServerOnly;
        public bool IsClientOnly => Location == Location.ClientOnly;
        public Property MirrorProperty { get; set; }
        internal bool ParentReference { get; set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public class WithPivotAttribute : Attribute
    {
    }

    public enum BuiltInType { String, Int32, Int64, Bool, DateTime, Object, Enum }
}
