using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceBuilder
    {
        private readonly SchemaStore schemaStore;

        public ResourceBuilder(SchemaStore schemaStore)
        {
            this.schemaStore = schemaStore;
        }
        public Resource FromClass<T>()
        {
            return FromType(typeof(T));
        }
        private Resource FromType(Type type, List<Type> parentResourceTypes = null)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                result.IsCollection = true;
                type = type.GetGenericArguments()[0];
            }

            result.Schema = schemaStore.FromType(type, true);
            result.Resoursces = new List<Resource>();
            var list = new List<Type>(parentResourceTypes ?? new List<Type>());
            list.Add(type);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    if (list.Contains(p.PropertyType))
                    {
                        throw new RecursionException("Recursive resources are not allowed.");
                    }
                    result.Resoursces.Add(FromType(p.PropertyType, list));
                }
            }
            return result;
        }
    }
}
