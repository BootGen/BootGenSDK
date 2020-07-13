using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class SchemaStore
    {
        private Dictionary<Type, Schema> schemasByType = new Dictionary<Type, Schema>();
        public List<Schema> Schemas { get; } = new List<Schema>();
        private Dictionary<Type, EnumSchema> enumSchemas = new Dictionary<Type, EnumSchema>();
        public List<EnumSchema> EnumSchemas => enumSchemas.Values.ToList();
        public Schema GetSchemaForResource(Type type)
        {
            return new SchemaBuilder(this).FromType(type);
        }
        internal bool TryGetValue(Type type, out Schema schema)
        {
            return schemasByType.TryGetValue(type, out schema);
        }
        internal void Add(Type type, Schema schema)
        {
            schemasByType.Add(type, schema);
            Schemas.Add(schema);
        }
        internal void Add(Schema schema)
        {
            Schemas.Add(schema);
        }
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
