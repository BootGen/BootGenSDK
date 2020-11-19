using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class EnumCollection
    {
        private Dictionary<Type, EnumModel> EnumsByType = new Dictionary<Type, EnumModel>();
        public List<EnumModel> Enums { get; } = new List<EnumModel>();
        internal bool TryGetValue(Type type, out EnumModel e)
        {
            return EnumsByType.TryGetValue(type, out e);
        }
        internal void Add(Type type, EnumModel e)
        {
            EnumsByType.Add(type, e);
            Add(e);
        }
        internal void Add(EnumModel e)
        {
            Enums.Add(e);
        }

    }
}
