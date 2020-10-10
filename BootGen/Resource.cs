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
        internal ParentRelation ParentRelation { get; set; }
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; set; }
        public bool HasTimestamps { get => Class.HasTimestamps; set => Class.HasTimestamps = value; }
        public bool Authenticate { get; set; }
        public bool IsReadonly { get; set; }
        public ResourceGenerationSettings GenerationSettings { get; } = new ResourceGenerationSettings();
        internal DataModel DataModel { get; set; }

        
        public Resource AddResource<T>(string parentName = null, bool manyToMany = false)
        {
            if (ParentResource != null)
                throw new Exception("Only a single layer of resource nesting is supported.");
            var parentRel = new ParentRelation(this, parentName);
            var classCount = DataModel.Classes.Count;
            Resource resource = FromClass<T>(parentRel);
            resource.Name = resource.Class.Name;
            resource.DataModel = DataModel;
            if (NestedResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A nested resource with name \"{resource.Name}\" already exists under \"{Name}\".");
            NestedResources.Add(resource);
            if (manyToMany)
            {
                ClassModel pivotClass = CreatePivot(this, resource);
                resource.Pivot = pivotClass;
            }

            var newClasses = DataModel.Classes.Skip(classCount).ToList();
            foreach (var c in newClasses)
            {
                c.MakePersisted();
            }
            return resource;
        }
        
        internal Resource FromClass<T>(ParentRelation parentRelation = null)
        {
            Resource resource = FromType(typeof(T), parentRelation);
            resource.Class.MakePersisted();
            return resource;
        }


        private Resource FromType(Type type, ParentRelation parentRelation = null)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Class = DataModel.TypeBuilder.FromType(type);
            result.Class.IsResource = true;
            result.NestedResources = new List<Resource>();
            result.ParentRelation = parentRelation;
            return result;
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
