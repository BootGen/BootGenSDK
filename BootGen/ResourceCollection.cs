using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceCollection
    {
        public List<RootResource> RootResources { get; } = new List<RootResource>();
        public List<Resource> Resources => Flatten(RootResources).ToList();

        public DataModel DataModel { get; }

        public ResourceCollection(DataModel dataModel)
        {
            DataModel = dataModel;

            var classes = new List<ClassModel>(DataModel.Classes);
            foreach (var c in classes)
                AddRootResource(c);
            foreach (var c in classes)
                AddNestedResources(c);
        }

        public void AddRootResource(ClassModel c)
        {
            var resource = new RootResource();
            resource.Name = c.Name;
            resource.Class = c;
            resource.Class.MakePersisted();
            resource.Class.IsResource = true;
            resource.DataModel = DataModel;
            if (RootResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
            RootResources.Add(resource);
        }
        public void AddNestedResources(ClassModel c)
        {
            var resource = RootResources.First(r => r.Class == c);
            foreach (var property in c.Properties)
            {
                if (!property.IsCollection || property.BuiltInType != BuiltInType.Object)
                    continue;
                if (property.IsManyToMany)
                {
                    CreateManyToManyRelation(resource, property);
                }
                else
                {
                    CreateOneToManyRelation(resource, property);
                }
            }
        }


        private void CreateOneToManyRelation(RootResource resource, Property property)
        {
            var rootResource = RootResources.First(r => r.Class == property.Class);
            var nestedResource = resource.OneToMany(property.Class);
            nestedResource.IsReadonly = true;
            nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }
        private void CreateManyToManyRelation(RootResource resource, Property property)
        {
            var rootResource = RootResources.First(r => r.Class == property.Class);
            var nestedResource = resource.ManyToMany(property.Class, $"{resource.Class.Name.Plural}{property.Class.Name.Plural}Pivot");
            nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }

        private IEnumerable<Resource> Flatten(List<RootResource> resources)
        {
            foreach (var r in resources)
            {
                yield return r;
                foreach (var sr in r.NestedResources)
                    yield return sr;
            }
        }

    }
}