using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class SchemaStore
    {
        private Dictionary<Type, Schema> schemas = new Dictionary<Type, Schema>();
        public List<Schema> Schemas => schemas.Values.ToList();
        public Schema GetSchemaForResource(Type type)
        {
            return new SchemaBuilder(this).FromType(type);
        }
        internal bool TryGetValue(Type type, out Schema schema)
        {
            return schemas.TryGetValue(type, out schema);
        }
        internal void Add(Type type, Schema schema)
        {
            schemas.Add(type, schema);
        }
    }
}
