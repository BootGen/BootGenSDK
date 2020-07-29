using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceBuilder
    {
        private readonly SchemaStore schemaStore;
        private readonly EnumSchemaStore enumSchemaStore;

        public ResourceBuilder(SchemaStore schemaStore, EnumSchemaStore enumSchemaStore)
        {
            this.schemaStore = schemaStore;
            this.enumSchemaStore = enumSchemaStore;
        }
        public Resource FromClass<T>(Resource parent = null)
        {
            List<Resource> parentResources = null;
            if (parent != null)
            {
                parentResources = new List<Resource>();
                while (parent != null)
                {
                    parentResources.Insert(0, parent);
                    parent = parent.ParentResource;
                }
            }
            Resource resource = FromType(typeof(T), parentResources);
            CheckDanglingResources(typeof(T));
            return resource;
        }

        private void CheckDanglingResources(Type type, HashSet<Type> checkedTypes = null)
        {
            checkedTypes = checkedTypes == null ? new HashSet<Type>() : new HashSet<Type>(checkedTypes);
            checkedTypes.Add(type);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }
            foreach (var p in type.GetProperties())
            {
                    if (!checkedTypes.Contains(p.PropertyType))
                        CheckDanglingResources(p.PropertyType, checkedTypes);
            }
        }

        private Resource FromType(Type type, List<Resource> parentResources = null)
        {
            var result = new Resource();
            result.SourceType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Schema = new SchemaBuilder(schemaStore, enumSchemaStore).FromType(type);
            if (result.Schema.IdProperty == null) {
                throw new InvalidResourceException("A resource must have an ID.");
            }
            result.NestedResources = new List<Resource>();
            result.ParentResources = parentResources ?? new List<Resource>();
            return result;
        }
    }
}
