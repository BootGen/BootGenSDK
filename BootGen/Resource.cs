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
        public Resource RootResource { get; set; }
        internal ParentRelation ParentRelation { get; set; }
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; set; }
        public bool HasTimestamps { get => Class.HasTimestamps; set => Class.HasTimestamps = value; }
        public bool Authenticate { get; set; }
        public bool IsReadonly { get; set; }
        public ResourceGenerationSettings GenerationSettings { get; } = new ResourceGenerationSettings();
        internal DataModel DataModel { get; set; }

        
        public Resource OneToMany<T>(string parentName = null)
        {
            if (ParentResource != null)
                throw new Exception("Only a single layer of resource nesting is supported.");
            Resource resource = DataModel.ResourceBuilder.FromClass<T>();
            resource.DataModel = DataModel;
            resource.ParentRelation = new ParentRelation(this, parentName);
            if (NestedResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A nested resource with name \"{resource.Name}\" already exists under \"{Name}\".");
            NestedResources.Add(resource);
            return resource;
        }
        
        public Resource ManyToMany<T>(string parentName = null)
        {
            Resource resource = OneToMany<T>(parentName);
            resource.Pivot = CreatePivot(this, resource);
            return resource;
        }
        
        private static ClassModel CreatePivot(Resource parent, Resource resource)
        {
            var pivotClass = new ClassModel
            {
                Name = resource.Name + "Pivot",
                Location = Location.ServerOnly,
                Properties = new List<Property> {
                        new Property {
                            Name = parent.Name + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = true
                        },
                        new Property {
                            Name = parent.Name,
                            BuiltInType = BuiltInType.Object,
                            Class = parent.Class,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Name + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Name,
                            BuiltInType = BuiltInType.Object,
                            Class = resource.Class,
                            IsRequired = true
                        }
                    }
            };
            pivotClass.MakePersisted();
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
