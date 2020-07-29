using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class EnumSchemaStore
    {
        private Dictionary<Type, EnumSchema> enumSchemas = new Dictionary<Type, EnumSchema>();
        public List<EnumSchema> EnumSchemas => enumSchemas.Values.ToList();
        internal bool TryGetValue(Type type, out EnumSchema schema)
        {
            return enumSchemas.TryGetValue(type, out schema);
        }
        internal void Add(Type type, EnumSchema schema)
        {
            enumSchemas.Add(type, schema);
        }

    }
}
