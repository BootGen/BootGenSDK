using System.Collections.Generic;

namespace BootGen
{
    public class Route
    {
        public string Path => PathModel.ToString();
        public Path PathModel { get; internal set; }
        public List<Operation> Operations { get; internal set; }
    }
}
