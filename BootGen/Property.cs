using System;
using System.ComponentModel;
using System.Linq;

namespace BootGen
{
    public class TypeDescription
    {
        public BuiltInType BuiltInType { get; set; }
        public bool IsCollection { get; set; }
        public Schema Schema { get; set; }
    }
    public class Property : TypeDescription
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool IsInternal { get; set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public enum BuiltInType { String, Int32, Int64, Bool, Object }
}
