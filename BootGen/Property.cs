using System;
using System.ComponentModel;
using System.Linq;

namespace BootGen
{
    public class Property
    {
        public string Name { get; internal set; }
        public BuiltInType Type { get; internal set; }
        public bool IsCollection { get; internal set; }
        public Schema Schema { get; internal set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public enum BuiltInType { String, Int32, Int64, Bool, Object }
}
