using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceCollection
    {
        public List<RootResource> RootResources { get; } = new List<RootResource>();

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
            string pivotName;
            if (string.Compare(resource.Class.Name, property.Class.Name, StringComparison.InvariantCulture) < 0)
                pivotName = $"{resource.Class.Name.Plural}{property.Class.Name.Plural}Pivot";
            else
                pivotName = $"{property.Class.Name.Plural}{resource.Class.Name.Plural}Pivot";
            var nestedResource = resource.ManyToMany(property.Class, pivotName);
            nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }

    }
}