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
                    pivot.Schema.Properties.First().MirrorProperty = property;
                    return pivot;
                }
                if (pivot.SchemaA == schemaB && pivot.SchemaB == schemaA)
                {
                    pivot.Schema.Properties.Last().MirrorProperty = property;
                    pivot.SchemaBExplicit = true;
                    return pivot;
                }
            }
            var p = new Pivot(schemaA, schemaB);
            p.SchemaAExplicit = true;
            p.Schema.Properties.First().MirrorProperty = property;
            Pivots.Add(p);
            return p;
        }
    }
}
