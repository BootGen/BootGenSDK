using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Resource
    {
        public Noun Name { get; set; }
        public ClassModel Class { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public Resource ParentResource => ParentRelation?.Resource;
        public string ParentName
        {
            get
            {
                return ParentRelation?.Name;
            }

            set
            {
                if (ParentRelation != null)
                    ParentRelation.Name = value;
            }
        }
        public Resource RootResource { get; set; }
        public bool IsRootResource => RootResource == this;
        internal ParentRelation ParentRelation { get; set; }
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; set; }
        public bool HasTimestamps { get => Class.HasTimestamps; set => Class.HasTimestamps = value; }
        public bool Authenticate { get; set; }
        public bool IsReadonly { get; set; }
        public ResourceGenerationSettings GenerationSettings { get; } = new ResourceGenerationSettings();
        internal DataModel DataModel { get; set; }

        public Resource OneToMany(Type type, Noun resourceName, string parentName = null)
        {
            if (ParentResource != null)
                throw new Exception("Only a single layer of resource nesting is supported.");
            Resource resource = DataModel.ResourceBuilder.FromType(type);
            if (resourceName != null)
                resource.Name = resourceName;
            resource.DataModel = DataModel;
            resource.ParentRelation = new ParentRelation(this, parentName);
            if (NestedResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A nested resource with name \"{resource.Name}\" already exists under \"{Name}\".");
            NestedResources.Add(resource);
            return resource;
        }
        
        public Resource ManyToMany(Type type, Noun resourceName, string pivotName)
        {
            Resource resource = OneToMany(type, resourceName);
            resource.Pivot = CreatePivot(this, resource, pivotName);
            return resource;
        }
        
        private ClassModel CreatePivot(Resource parent, Resource resource, string name)
        {
            var pivotClass = DataModel.Classes.FirstOrDefault(c => c.Name == name);
            if (pivotClass != null)
                return pivotClass;
            string name1 = parent.Name;
            string name2 = resource.Name;
            if (name1 == name2) {
                name1 += "1";
                name2 += "2";
            }
            pivotClass = new ClassModel
            {
                Name = name,
                Location = Location.ServerOnly,
                Properties = new List<Property> {
                        new Property {
                            Name = name1 + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = true
                        },
                        new Property {
                            Name = name1,
                            BuiltInType = BuiltInType.Object,
                            Class = parent.Class,
                            IsRequired = true
                        },
                        new Property {
                            Name = name2 + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = true
                        },
                        new Property {
                            Name = name2,
                            BuiltInType = BuiltInType.Object,
                            Class = resource.Class,
                            IsRequired = true
                        }
                    }
            };
            pivotClass.MakePersisted();
            DataModel.ClassCollection.Add(pivotClass);
            return pivotClass;
        }

    }

    public class ResourceGenerationSettings
    {        
        public bool GenerateController { get; set; } = true;
        public bool GenerateServiceInterface { get; set; } = true;
        public bool GenerateService { get; set; } = true;
        public string ControllerName { get; set; }
        public string ServiceName { get; set; }
    }

}
