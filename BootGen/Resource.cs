using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Resource
    {
        public string Name => Schema.Name;
        public bool IsCollection { get; private set; }
        public Schema Schema { get; private set; }
        public bool Get { get; set; }
        public bool Put { get; set; }
        public bool Patch { get; set; }
        public bool Delete { get; set; }

        public List<Resource> Resoursces { get; private set; }

        public static Resource FromClass<T>()
        {
            return FromType(typeof(T));
        }

        private static Resource FromType(Type type, List<Type> parentResourceTypes = null)
        {
            var result = new Resource();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                result.IsCollection = true;
                type = type.GetGenericArguments()[0];
            }

            result.Schema = Schema.FromType(type);
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
                    result.Resoursces.Add(FromType(p.PropertyType));
                }
            }
            return result;
        }
    }

    public class RecursionException : Exception
    {
        public RecursionException(string message) : base(message)
        {

        }
    }
}
