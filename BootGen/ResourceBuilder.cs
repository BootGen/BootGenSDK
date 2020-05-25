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
            Resource resource = FromType(typeof(T));
            CheckDanglingResources(typeof(T));
            return resource;
        }

        private void CheckDanglingResources(Type type, bool parentIsResource = true, HashSet<Type> checkedTypes = null)
        {
            checkedTypes = checkedTypes == null ? new HashSet<Type>() : new HashSet<Type>(checkedTypes);
            checkedTypes.Add(type);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    if (parentIsResource)
                    {
                        if (!checkedTypes.Contains(p.PropertyType))
                            CheckDanglingResources(p.PropertyType, true, checkedTypes);
                    }
                    else
                    {
                        throw new IllegalNestingException();
                    }
                }
                else
                {
                    if (!checkedTypes.Contains(p.PropertyType))
                        CheckDanglingResources(p.PropertyType, false, checkedTypes);
                }
            }
        }

        private Resource FromType(Type type, List<Resource> parentResources = null)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                result.IsCollection = true;
                type = type.GetGenericArguments()[0];
            }

            result.Schema = schemaStore.GetSchemaForResource(type);
            result.NestedResources = new List<Resource>();
            result.ParentResources = parentResources ?? new List<Resource>();
            parentResources = new List<Resource>(result.ParentResources);
            parentResources.Add(result);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    if (parentResources.Any(r => r.Name == p.PropertyType.Name.Split('.').Last()))
                    {
                        throw new RecursionException("Recursive resources are not allowed.");
                    }
                    result.NestedResources.Add(FromType(p.PropertyType, parentResources));
                }
            }
            return result;
        }
    }
}
