using System.Collections.Generic;

namespace BootGen
{
    class PivotStore
    {
        public List<Pivot> Pivots { get; } = new List<Pivot>();
        public Pivot Add(Schema schemaA, Schema schemaB)
        {
            foreach (var pivot in Pivots)
            {
                if (pivot.SchemaA == schemaA && pivot.SchemaB == schemaB)
                {
                    pivot.SchemaAExplicit = true;
                    return pivot;
                }
                if (pivot.SchemaA == schemaB && pivot.SchemaB == schemaA)
                {
                    pivot.SchemaBExplicit = true;
                    return pivot;
                }
            }
            var p = new Pivot(schemaA, schemaB);
            p.SchemaAExplicit = true;
            Pivots.Add(p);
            return p;
        }
    }
}
