using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Schema
    {
        public string Name { get; internal set; }
        public Property IdProperty { get; internal set; }
        public List<Property> Properties { get; internal set; }
        public bool IsResource { get; internal set; }
        public bool HasRequiredProperties => Properties.Any(p => p.IsRequired);
    }
}
