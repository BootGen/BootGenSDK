using System.Collections.Generic;

namespace BootGen
{
    public class Schema
    {
        public string Name { get; internal set; }
        public Property IdProperty { get; internal set; }
        public List<Property> Properties { get; internal set; }
        public bool IsResource { get; internal set; }
    }
}
