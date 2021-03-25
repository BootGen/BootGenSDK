using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public abstract class Resource
    {
        public Noun Name { get; set; }
        public ClassModel Class { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public bool HasTimestamps { get => Class.HasTimestamps; set => Class.HasTimestamps = value; }
        public bool IsReadonly { get; set; }
        internal DataModel DataModel { get; set; }

    }

    public class RootResource : Resource
    {
        public List<NestedResource> NestedResources { get; } = new List<NestedResource>();
        public List<NestedResource> AlternateResources { get; } = new List<NestedResource>();

        public NestedResource OneToMany(Property property)
        {
            NestedResource resource = new NestedResource();
            resource.Name = property.Noun;
            resource.Class = property.Class;
            resource.DataModel = DataModel;
            resource.ParentRelation = new ParentRelation(this);
            if (NestedResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A nested resource with name \"{resource.Name}\" already exists under \"{Name}\".");
            NestedResources.Add(resource);
            return resource;
        }
        
        public NestedResource ManyToMany(Property property, string pivotName)
        {
            NestedResource resource = OneToMany(property);
            resource.Pivot = CreatePivot(this, resource, pivotName);
            return resource;
        }
        private ClassModel CreatePivot(Resource parent, Resource resource, string name)
        {
            var pivotClass = DataModel.Classes.FirstOrDefault(c => c.Name == name);
            if (pivotClass != null)
                return pivotClass;
            var name1 = parent.Name.Singular;
            var name2 = resource.Name.Singular;
            pivotClass = new ClassModel(name)
            {
                Location = PropertyType.ServerOnly
            };
            pivotClass.Properties.Add(new Property {
                            Name = name1 + "Id",
                            BuiltInType = BuiltInType.Int
                        });
            pivotClass.Properties.Add(new Property {
                            Name = name1,
                            Noun = name1,
                            BuiltInType = BuiltInType.Object,
                            Class = parent.Class
                        });
            pivotClass.Properties.Add(new Property {
                            Name = name2 + "Id",
                            BuiltInType = BuiltInType.Int
                        });
            pivotClass.Properties.Add(new Property {
                            Name = name2,
                            Noun = name2,
                            BuiltInType = BuiltInType.Object,
                            Class = resource.Class
                        });
            DataModel.ClassCollection.Add(pivotClass);
            return pivotClass;
        }

    }
    public class NestedResource : Resource
    {
        internal ParentRelation ParentRelation { get; set; }
        public Resource ParentResource => ParentRelation?.Resource;
        public RootResource RootResource { get; set; }
        public ClassModel Pivot { get; set; }
        public string ParentName =>  ParentRelation?.Name;
    }
}
