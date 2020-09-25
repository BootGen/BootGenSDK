using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class ResourceBuilder
    {
        private readonly ClassStore classStore;
        private readonly EnumStore enumStore;

        public ResourceBuilder(ClassStore classStore, EnumStore enumStore)
        {
            this.classStore = classStore;
            this.enumStore = enumStore;
        }
        public Resource FromClass<T>(ParentRelation parentRelation = null)
        {
            Resource resource = FromType(typeof(T), parentRelation);
            CheckDanglingResources(typeof(T));
            if (resource.Class.Properties.All(p => p.Name != "Id"))
                resource.Class.Properties.Insert(0, new Property
                {
                    Name = "Id",
                    BuiltInType = BuiltInType.Int32,
                    IsRequired = true,
                    IsClientReadonly = true
                });
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

        private Resource FromType(Type type, ParentRelation parentRelation = null)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Class = new TypeBuilder(classStore, enumStore).FromType(type);
            result.Class.IsResource = true;
            result.NestedResources = new List<Resource>();
            result.ParentRelation = parentRelation;
            return result;
        }
    }
}
