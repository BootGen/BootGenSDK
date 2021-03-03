using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        }

        public RootResource Add<T>()
        {
            return Add(typeof(T));
        }

        public RootResource Add(ClassModel c)
        {
            var resource = new RootResource();
            resource.Name = c.Name;
            resource.Class = c;
            resource.Class.MakePersisted();
            resource.Class.IsResource = true;
            resource.DataModel = DataModel;
            AddRootResource(resource);

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

        private RootResource Add(Type type)
        {
            RootResource resource = DataModel.ResourceBuilder.FromType<RootResource>(type);
            resource.DataModel = DataModel;
            AddRootResource(resource);

            foreach (var property in type.GetProperties())
            {
                var oneToManyAttribute = property.Get<OneToManyAttribute>();
                if (oneToManyAttribute != null)
                {
                    CreateOneToManyRelation(resource, property, oneToManyAttribute);
                }
                var manyToManyAttribute = property.Get<ManyToManyAttribute>();
                if (manyToManyAttribute != null)
                {
                    CreateManyToManyRelation(resource, property, manyToManyAttribute);
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

        private void CreateOneToManyRelation(RootResource resource, System.Reflection.PropertyInfo property, System.Reflection.CustomAttributeData oneToManyAttribute)
        {
            var propertyType = property.PropertyType;
            Type genericType;
            try
            {
                genericType = propertyType.GetGenericTypeDefinition();
            }
            catch
            {
                throw new Exception("A one-to-many reference must be a list.");
            }
            if (genericType != typeof(List<>))
            {
                throw new Exception("A one-to-many reference must be a list.");
            }
            propertyType = propertyType.GetGenericArguments()[0];
            var rootResource = RootResources.FirstOrDefault(r => r.Class.Name == propertyType.Name);
            if (rootResource == null)
                rootResource = Add(propertyType);
            var parentName = oneToManyAttribute.GetFirstParameter<string>();
            var nestedResource = resource.OneToMany(propertyType, GetResourceName(property), parentName);
            nestedResource.IsReadonly = true;
            var singularNameAttribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(SingularNameAttribute));
            var singularName = singularNameAttribute?.GetFirstParameter<string>();
            nestedResource.Name = singularName ?? property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }

        private static Noun GetResourceName(PropertyInfo property)
        {
            var singularName = property.Get<SingularNameAttribute>()?.GetFirstParameter<string>();
            Noun resourceName = singularName ?? property.Name.Substring(0, property.Name.Length - 1);
            resourceName.Plural = property.Name;
            return resourceName;
        }

        private void CreateManyToManyRelation(RootResource resource, PropertyInfo property, CustomAttributeData manyToManyAttribute)
        {
            var propertyType = property.PropertyType;
            Type genericType;
            try
            {
                genericType = propertyType.GetGenericTypeDefinition();
            }
            catch
            {
                throw new Exception("A many-to-many reference must be a list.");
            }
            if (genericType != typeof(List<>))
            {
                throw new Exception("A many-to-many reference must be a list.");
            }
            propertyType = propertyType.GetGenericArguments()[0];
            var rootResource = RootResources.FirstOrDefault(r => r.Class.Name == propertyType.Name);
            if (rootResource == null)
                rootResource = Add(propertyType);
            var pivotName = manyToManyAttribute.GetFirstParameter<string>();
            var nestedResource = resource.ManyToMany(propertyType, GetResourceName(property), pivotName);
            var singularNameAttribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(SingularNameAttribute));
            var singularName = singularNameAttribute?.GetFirstParameter<string>();
            nestedResource.Name = singularName ?? property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
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