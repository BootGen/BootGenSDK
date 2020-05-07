using System.Collections.Generic;

namespace BootGen
{
    public class Route
    {
        public string Path { get; internal set; }
        public List<Operation> Operations { get; internal set; }
    }
}
