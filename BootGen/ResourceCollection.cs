using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Pluralize.NET;

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
                Add(c);
        }

        public RootResource Add(ClassModel c)
        {

            var resource = RootResources.FirstOrDefault(r => r.Class == c);
            if (resource == null)
            {
                resource = new RootResource();
                resource.Name = c.Name;
                resource.Class = c;
                resource.Class.MakePersisted();
                resource.Class.IsResource = true;
                resource.DataModel = DataModel;
                AddRootResource(resource);
            }

            foreach (var property in c.Properties)
            {
                if (!property.IsCollection || property.BuiltInType != BuiltInType.Object)
                    continue;
                if (property.IsManyToMany)
                {
                    CreateManyToManyRelation(resource, property);
                } else
                {
                    CreateOneToManyRelation(resource, property);
                }
            }
            return resource;
        }


        private void AddRootResource(RootResource resource)
        {
            if (RootResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
            RootResources.Add(resource);
        }
        private void CreateOneToManyRelation(RootResource resource, Property property)
        {
            var rootResource = RootResources.FirstOrDefault(r => r.Class == property.Class);
            if (rootResource == null)
                rootResource = Add(property.Class);
            var nestedResource = resource.OneToMany(property.Class);
            nestedResource.IsReadonly = true;
            nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }
        private void CreateManyToManyRelation(RootResource resource, Property property)
        {
            var rootResource = RootResources.FirstOrDefault(r => r.Class == property.Class);
            if (rootResource == null)
                rootResource = Add(property.Class);
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