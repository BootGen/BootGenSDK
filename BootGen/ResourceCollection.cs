using System;
using System.Collections.Generic;
using System.Linq;

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
                var oneToManyAttribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(OneToManyAttribute));
                if (oneToManyAttribute != null)
                {
                    var propertyType = property.PropertyType;
                    Type genericType;
                    try {
                        genericType = propertyType.GetGenericTypeDefinition();
                    } catch {
                        throw new Exception("A one-to-many reference must be a list.");
                    }
                    if (genericType == typeof(List<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                        if (RootResources.All(r => r.Class.Name != propertyType.Name))
                            Add(propertyType);
                        var parentName = oneToManyAttribute.ConstructorArguments.FirstOrDefault().Value as string;
                        var nestedResource = resource.OneToMany(propertyType, parentName);
                        var singularNameAttribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(SingularNameAttribute));
                        var singularName = singularNameAttribute?.ConstructorArguments.First().Value as string;
                        nestedResource.Name = singularName ?? property.Name.Substring(0, property.Name.Length-1);
                        nestedResource.Name.Plural = property.Name;
                    }
                    else
                    {
                        throw new Exception("A one-to-many reference must be a list.");
                    }
                }
            }

            return resource;
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