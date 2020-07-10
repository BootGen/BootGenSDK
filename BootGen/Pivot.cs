using System.Collections.Generic;

namespace BootGen
{
    public class Pivot
    {
        public string Name => SchemaA.Name + SchemaB.Name;
        public Schema SchemaA { get; }
        public bool SchemaAExplicit { get; set; }
        public Schema SchemaB { get; }
        public bool SchemaBExplicit { get; set; }
        public Schema Schema { get; }

        public Pivot(Schema schemaA, Schema schemaB)
        {
            SchemaA = schemaA;
            SchemaB = schemaB;
            Schema = new Schema();
            Schema.Name = SchemaA.Name + SchemaB.Name;
            Schema.IdProperty = new Property
            {
                Name = "Id",
                BuiltInType = BuiltInType.Int32
            };
            Schema.Properties = new List<Property> { Schema.IdProperty };
            Schema.Properties.Add(new Property
            {
                Name = SchemaA.Name,
                BuiltInType = BuiltInType.Object,
                Schema = SchemaA
            });
            Schema.Properties.Add(new Property
            {
                Name = SchemaB.Name,
                BuiltInType = BuiltInType.Object,
                Schema = SchemaB
            });
        }
    }
}
