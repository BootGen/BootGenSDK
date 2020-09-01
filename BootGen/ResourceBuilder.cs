using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceBuilder
    {
        private readonly ClassStore classStore;
        private readonly EnumStore enumStore;

        public ResourceBuilder(ClassStore classStore, EnumStore enumStore)
        {
            this.classStore = classStore;
            this.enumStore = enumStore;
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
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Class = new TypeBuilder(classStore, enumStore).FromType(type);
            result.Class.IsResource = true;
            result.Class.Properties.Insert(0, new Property {
                Name = "Uuid",
                BuiltInType = BuiltInType.Guid,
                IsRequired = true
            });
            result.NestedResources = new List<Resource>();
            result.ParentResources = parentResources ?? new List<Resource>();
            return result;
        }
    }
}
