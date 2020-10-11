using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class ResourceBuilder
    {
        private readonly TypeBuilder typeBuilder;

        public ResourceBuilder(TypeBuilder typeBuilder)
        {
            this.typeBuilder = typeBuilder;
        }
        public Resource FromClass<T>()
        {
            return FromType(typeof(T));
        }


        private Resource FromType(Type type)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            result.Class = typeBuilder.FromType(type);
            result.Class.IsResource = true;
            result.NestedResources = new List<Resource>();
            result.Name = result.Class.Name;
            return result;
        }
    }
}
