using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class EnumCollection
    {
        public List<EnumModel> Enums { get; } = new List<EnumModel>();
        internal void Add(EnumModel e)
        {
            if (!Enums.Contains(e))
                Enums.Add(e);
        }

    }
}
