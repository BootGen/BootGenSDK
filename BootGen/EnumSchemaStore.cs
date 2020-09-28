using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class EnumStore
    {
        private Dictionary<Type, EnumModel> enums = new Dictionary<Type, EnumModel>();
        public List<EnumModel> Enums => enums.Values.ToList();
        internal bool TryGetValue(Type type, out EnumModel e)
        {
            return enums.TryGetValue(type, out e);
        }
        internal void Add(Type type, EnumModel e)
        {
            enums.Add(type, e);
        }

    }
}
