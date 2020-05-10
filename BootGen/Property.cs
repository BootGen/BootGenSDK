using System;
using System.ComponentModel;
using System.Linq;

namespace BootGen
{
    public class TypeDescription
    {
        public BuiltInType BuiltInType { get; internal set; }
        public bool IsCollection { get; internal set; }
        public Schema Schema { get; internal set; }
    }
    public class Property : TypeDescription
    {
        public string Name { get; internal set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public enum BuiltInType { String, Int32, Int64, Bool, Object }
}
