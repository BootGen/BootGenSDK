using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ClassModel
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string PluralName { get; internal set; }
        public List<Property> Properties { get; internal set; }
        public List<Property> ServerProperties => Properties.Where(p => p.Location != Location.ClientOnly).ToList();
        public List<Property> ClientProperties => Properties.Where(p => p.Location != Location.ServerOnly).ToList();
        public List<Property> CommonProperties => Properties.Where(p => p.Location == Location.Both).ToList();
        public bool HasRequiredProperties => Properties.Any(p => p.IsRequired);
        public Location Location { get; set; }
        public bool IsResource { get; set; }
        public bool Persisted { get; set; }
        public bool HasTimestamps { get; internal set; }
        public bool ConcurrencyControl { get; internal set; }
        public Property IdProperty => PropertyWithName("Id");

        public Property PropertyWithName(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }
    }
}
