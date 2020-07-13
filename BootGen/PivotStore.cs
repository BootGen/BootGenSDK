using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    class PivotStore
    {
        public List<Pivot> Pivots { get; } = new List<Pivot>();
        public Pivot Add(Property property, Schema schemaB)
        {
            var schemaA = property.Schema;
            
            foreach (var pivot in Pivots)
            {
                if (pivot.SchemaA == schemaA && pivot.SchemaB == schemaB)
                {
                    pivot.SchemaAExplicit = true;
                    AddMirrorProperties(property, schemaB, pivot);
                    return pivot;
                }
                if (pivot.SchemaA == schemaB && pivot.SchemaB == schemaA)
                {
                    AddMirrorProperties(property, schemaA, pivot);
                    pivot.SchemaBExplicit = true;
                    return pivot;
                }
            }
            var p = new Pivot(schemaA, schemaB);
            p.SchemaAExplicit = true;
            AddMirrorProperties(property, schemaB, p);
            Pivots.Add(p);
            return p;
        }

        private static void AddMirrorProperties(Property property, Schema schemaA, Pivot pivot)
        {
            var pr = pivot.Schema.Properties.First(prop => prop.Schema == schemaA);

            var mirrorProperty = new Property
            {
                Name = pivot.Name + "s",
                BuiltInType = BuiltInType.Object,
                Schema = pivot.Schema,
                IsCollection = true,
                Location = Location.ServerOnly
            };
            property.ParentSchema.Properties.Add(mirrorProperty);
            mirrorProperty.MirrorProperty = pr;
            pr.MirrorProperty = mirrorProperty;
        }
    }
}
