using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BootGen
{
    public class ResourceCollection
    {
        public List<Resource> RootResources { get; } = new List<Resource>();
        public List<Resource> Resources => Flatten(RootResources).ToList();

        public DataModel DataModel { get; }

        public ResourceCollection(DataModel dataModel)
        {
            DataModel = dataModel;
        }

        public Resource Add<T>()
        {
            return Add(typeof(T));
        }

        private Resource Add(Type type)
        {
            Resource resource = DataModel.ResourceBuilder.FromType(type);
            resource.DataModel = DataModel;
            resource.RootResource = resource;
            if (RootResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
            RootResources.Add(resource);

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

        private void CreateOneToManyRelation(Resource resource, System.Reflection.PropertyInfo property, System.Reflection.CustomAttributeData oneToManyAttribute)
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
        }

        private static Noun GetResourceName(PropertyInfo property)
        {
            var singularName = property.Get<SingularNameAttribute>()?.GetFirstParameter<string>();
            Noun resourceName = singularName ?? property.Name.Substring(0, property.Name.Length - 1);
            resourceName.Plural = property.Name;
            return resourceName;
        }

        private void CreateManyToManyRelation(Resource resource, PropertyInfo property, CustomAttributeData manyToManyAttribute)
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
    }
}