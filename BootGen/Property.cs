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
        public ClassModel Class { get; set; }
        public EnumModel Enum { get; set; }
    }

    public enum PropertyType { Normal, ServerOnly, Virtual }

    public class Property : TypeDescription
    {
        public Noun Noun { get; set; }
        public string Name { get; set; }
        public PropertyType PropertyType { get; set; }
        public bool IsServerOnly => PropertyType == PropertyType.ServerOnly;
        public Property MirrorProperty { get; set; }
        public bool IsParentReference { get; set; }
        public bool IsManyToMany { get; set; }
        public bool IsClientReadonly { get; set; }
        internal Resource IdReferenceToParent { get; set; }
    }
    

    public enum BuiltInType { String, Int32, Int64, Float, Double, Bool, DateTime, Guid, Object, Enum }
}
