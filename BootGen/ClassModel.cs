using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    /// <summary>Represents a class used in the REST API</summary>
    public class ClassModel
    {
        static public string IdName { get; set; } = "Id";
        public int Id { get; set; }
        public Noun Name { get; set; }
        public List<Property> Properties { get; }
        public bool IsServerOnly { get; set; }
        public bool HasTimestamps { get; set; }
        public bool IsPivot { get; set; }
        public bool IsRoot { get; set; }
        public bool ReferredSingle { get; set; }
        public bool ReferredPlural { get; set; }
        public Property IdProperty => PropertyWithName(ClassModel.IdName);
        public List<Property> CommonProperties => Properties.Where(p => !p.IsServerOnly).ToList();
        public List<Property> BaseProperties => CommonProperties.Where(p => !p.IsKey).ToList();
        public List<Property> JsonProperties => Properties.Where(p => !p.IsKey && !p.IsParentReference).ToList();
        public List<Property> ChildReferences => Properties.Where(p => p.BuiltInType == BuiltInType.Object && !p.IsParentReference).ToList();
        public bool HasChild => ChildReferences.Any();

        public ClassModel(string name)
        {
            Name = name;
            Properties = new List<Property> {
                new Property
                {
                    Name = ClassModel.IdName,
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
