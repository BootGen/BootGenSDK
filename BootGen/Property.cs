using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BootGen
{
    public class TypeDescription
    {
        public BuiltInType BuiltInType { get; set; }
        public bool IsCollection { get; set; }
        public Schema Schema { get; set; }
        public EnumSchema EnumSchema { get; set; }
    }

    public enum Location { Both, ServerOnly, ClientOnly }

    public class Property : TypeDescription
    {
        public Schema ParentSchema { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public Location Location { get; set; }
        public bool IsServerOnly => Location == Location.ServerOnly;
        public bool IsClientOnly => Location == Location.ClientOnly;
        public List<string> Tags { get; } = new List<string>();
        public bool HasTag(string tag) => Tags.Contains(tag);
        public Property MirrorProperty { get; set; }
        public Pivot Pivot { get; set; }
        internal bool WithPivot { get; set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public class WithPivotAttribute : Attribute
    {
    }

    public class Pivot
    {
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

    public enum BuiltInType { String, Int32, Int64, Bool, DateTime, Object, Enum }
}
