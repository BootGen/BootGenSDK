using System.Collections.Generic;

namespace BootGen
{
    class PivotStore
    {
        public List<Pivot> Pivots { get; } = new List<Pivot>();
        public Pivot Add(Schema schemaA, Schema schemaB)
        {
            foreach (var pivot in Pivots)
                if (pivot.SchemaA == schemaA && pivot.SchemaB == schemaB || pivot.SchemaA == schemaB && pivot.SchemaB == schemaA)
                    return pivot;
            var p = new Pivot{ SchemaA = schemaA, SchemaB = schemaB};
            Pivots.Add(p);
            return p;
        }
    }
}
