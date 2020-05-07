using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class OASSchema 
    {
        public string Name { get; set; }
        public List<OASProperty> Properties { get; set; }
        public bool HasRequiredProperties => Properties.Any(p => p.Required);
    }
}