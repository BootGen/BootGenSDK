using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    /// <summary>Represents a class used in the REST API</summary>
    public class ClassModel
    {
        public int Id { get; set; }
        public Noun Name { get; set; }
        public List<Property> Properties { get; set; }
        public Property IdProperty => PropertyWithName("Id");

        public Property PropertyWithName(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public List<Property> ServerProperties => Properties.Where(p => p.PropertyType != PropertyType.Virtual).ToList();

        internal void MakePersisted()
        {
            if (Properties.All(p => p.Name != "Id"))
                    Properties.Insert(0, new Property
                    {
                        Name = "Id",
                        BuiltInType = BuiltInType.Int32,
                        IsRequired = true,
                        IsClientReadonly = true
                    });
            Persisted = true;
        }

        public List<Property> CommonProperties => Properties.Where(p => p.PropertyType == PropertyType.Normal).ToList();

        public bool HasRequiredProperties => Properties.Any(p => p.IsRequired);

        public PropertyType Location { get; set; }

        public bool IsResource { get; set; }

        public bool Persisted { get; set; }

        public bool HasTimestamps { get; set; }
        internal bool RelationsAreSetUp { get; set; }
    }
}
