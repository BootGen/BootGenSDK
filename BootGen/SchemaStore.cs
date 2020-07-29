using System;
using System.Collections.Generic;

namespace BootGen
{
    public class SchemaStore
    {
        private Dictionary<Type, Schema> schemasByType = new Dictionary<Type, Schema>();
        public List<Schema> Schemas { get; } = new List<Schema>();

        internal bool TryGetValue(Type type, out Schema schema)
        {
            return schemasByType.TryGetValue(type, out schema);
        }
        internal void Add(Type type, Schema schema)
        {
            schemasByType.Add(type, schema);
            Add(schema);
        }
        internal void Add(Schema schema)
        {
            schema.Id = Schemas.Count;
            Schemas.Add(schema);
        }
    }
}
