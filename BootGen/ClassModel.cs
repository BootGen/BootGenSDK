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
        public List<Property> Properties { get; }
        public Property IdProperty => PropertyWithName("Id");

        public ClassModel(string name)
        {
            Name = name;
            Properties = new List<Property> {
                new Property
                {
                    Name = "Id",
                    BuiltInType = BuiltInType.Int32,
                    IsClientReadonly = true
                }
            };
        }

        public Property PropertyWithName(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public List<Property> ServerProperties => Properties.Where(p => p.PropertyType != PropertyType.Virtual).ToList();

        public List<Property> CommonProperties => Properties.Where(p => p.PropertyType == PropertyType.Normal).ToList();

        public PropertyType Location { get; set; }

        public bool IsResource { get; set; }

        public bool HasTimestamps { get; set; }
        internal bool RelationsAreSetUp { get; set; }
    }
}
