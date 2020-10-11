using System;
using System.Collections.Generic;

namespace BootGen
{
    internal class ClassCollection
    {
        private Dictionary<Type, ClassModel> classesByType = new Dictionary<Type, ClassModel>();
        public List<ClassModel> Classes { get; } = new List<ClassModel>();

        internal bool TryGetValue(Type type, out ClassModel c)
        {
            return classesByType.TryGetValue(type, out c);
        }
        internal void Add(Type type, ClassModel c)
        {
            classesByType.Add(type, c);
            Add(c);
        }
        internal void Add(ClassModel c)
        {
            if (Classes.Contains(c))
                return;
            c.Id = Classes.Count;
            Classes.Add(c);
        }
    }
}
