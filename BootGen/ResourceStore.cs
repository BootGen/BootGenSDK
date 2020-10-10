using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceStore
    {
        public List<Resource> RootResources { get; } = new List<Resource>();
        public List<Resource> Resources => Flatten(RootResources).ToList();
        private readonly ResourceBuilder resourceBuilder;
        internal ClassStore ClassStore { get; }
        internal EnumStore EnumStore { get; }

        public ResourceStore()
        {
            ClassStore = new ClassStore();
            EnumStore = new EnumStore();
            resourceBuilder = new ResourceBuilder(ClassStore, EnumStore);
        }
        
        public Resource AddResource<T>(Resource parent = null, string parentName = null, bool manyToMany = false)
        {
            if (parent?.ParentResource != null)
                throw new Exception("Only a single layer of resource nesting is supported.");
            ParentRelation parentRel = null;
            if (parent != null)
                parentRel = new ParentRelation(parent, parentName);
            var classCount = ClassStore.Classes.Count;
            Resource resource = resourceBuilder.FromClass<T>(parentRel);
            resource.Name = resource.Class.Name;
            resource.PluralName = resource.Class.PluralName;
            if (parent == null)
            {
                if (RootResources.Any(r => r.Name == resource.Name))
                    throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
                RootResources.Add(resource);
            }
            else
            {
                if (parent.NestedResources.Any(r => r.Name == resource.Name))
                    throw new Exception($"A nested resource with name \"{resource.Name}\" already exists under \"{parent.Name}\".");
                parent.NestedResources.Add(resource);
            }
            if (manyToMany)
            {
                ClassModel pivotClass = CreatePivot(parent, resource);
                resource.Pivot = pivotClass;
            }

            var newClasses = ClassStore.Classes.Skip(classCount).ToList();
            foreach (var c in newClasses)
            {
                if (c.Properties.All(p => p.Name != "Id"))
                    c.Properties.Insert(0, new Property
                    {
                        Name = "Id",
                        BuiltInType = BuiltInType.Int32,
                        IsRequired = true,
                        IsClientReadonly = true
                    });
                c.Persisted = true;
            }
            return resource;
        }

        private static ClassModel CreatePivot(Resource parent, Resource resource)
        {
            var pivotClass = new ClassModel
            {
                Name = resource.Name + "Pivot",
                PluralName = resource.Name + "Pivots",
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

        private IEnumerable<Resource> Flatten(List<Resource> resources)
        {
            foreach (var r in resources)
            {
                yield return r;
                foreach (var sr in Flatten(r.NestedResources))
                    yield return sr;
            }
        }
        public Resource GetRootResource(Resource resource)
        {
            return RootResources.FirstOrDefault(r => r.Class == resource.Class);
        }
    }
}