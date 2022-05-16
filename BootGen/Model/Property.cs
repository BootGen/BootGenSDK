using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BootGen
{

    public class Property
    {
        public Noun Noun { get; set; }
        public string Name { get; set; }
        public BuiltInType BuiltInType { get; set; }
        public bool IsCollection { get; set; }
        public ClassModel Class { get; set; }
        public bool IsServerOnly { get; set; }
        public Property MirrorProperty { get; set; }
        public bool IsParentReference { get; set; }
        public bool IsManyToMany { get; set; }
        public bool IsClientReadonly { get; set; }
        public bool IsKey { get; set; }
    }
    

    public enum BuiltInType { String, Int, Float, Bool, DateTime, Object }
}