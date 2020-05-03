using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Resource
    {
        public string Name => Schema.Name;
        public bool IsCollection { get;  private set; }
        public Schema Schema { get; private set; }
        public bool Get { get; set; }
        public bool Put { get; set; }
        public bool Patch { get; set; }
        public bool Delete { get; set; }

        public List<Resource> Resoursces { get; private set; }

        public static Resource FromClass<T>() {
            Type type = typeof(T);
            var result = new Resource();
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                result.IsCollection = true;
                type = type.GetGenericArguments()[0];
            }
            result.Schema = new Schema();
            result.Schema.Name = type.Name.Split('.').Last();
            result.Schema.Properties = new List<Property>();
            foreach ( var p in type.GetProperties())
            {
                result.Schema.Properties.Add(new Property { Name = p.Name, Type = GetType(p.PropertyType)});
            }
            return result;
        }

        private static BuiltInType GetType(Type type)
        {
            switch (type.ToString().Split('.').Last().ToLower())
            {
                case "string":
                  return BuiltInType.String;
                case "int":
                  return BuiltInType.Int32;
                case "long":
                  return BuiltInType.Int64;
                case "bool":
                  return BuiltInType.Bool;
                default:
                  return BuiltInType.Object;
            }
        }
    }
}
