using System.Collections.Generic;

namespace BootGen
{
    public class EnumModel {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public List<string> Values { get; internal set; }
    }
}
