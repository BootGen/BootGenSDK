using System;
using System.Collections.Generic;

namespace BootGen
{
    internal class ClassCollection
    {
        public List<ClassModel> Classes { get; } = new List<ClassModel>();

        internal void Add(ClassModel c)
        {
            if (Classes.Contains(c))
                return;
            c.Id = Classes.Count;
            Classes.Add(c);
        }
    }
}
