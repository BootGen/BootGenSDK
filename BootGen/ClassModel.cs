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
        public bool IsServerOnly { get; set; }
        public bool HasTimestamps { get; set; }
        public bool IsPivot { get; set; }
        public Property IdProperty => PropertyWithName("Id");
        public List<Property> CommonProperties => Properties.Where(p => !p.IsServerOnly).ToList();


        public ClassModel(string name)
        {
            Name = name;
            Properties = new List<Property> {
                new Property
                {
                    Name = "Id",
                    BuiltInType = BuiltInType.Int,
                    IsClientReadonly = true,
                    IsKey = true
                }
            };
        }

        public Property PropertyWithName(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }
    }
}
