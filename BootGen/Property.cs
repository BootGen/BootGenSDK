using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class Property
    {
        public string Name { get; internal set; }
        public BuiltInType Type { get; internal set; }
        public bool IsCollection { get; internal set; }
        public Schema Schema { get; internal set; }
    }

    public class ResourceAttribute : Attribute
    {

    }

    public class Schema
    {
        public string Name { get; internal set; }
        public List<Property> Properties { get; internal set; }
    }

    public class SchemaStore
    {
        private Dictionary<Type, Schema> schemas = new Dictionary<Type, Schema>();
        public List<Schema> Schemas => schemas.Values.ToList();
        internal Schema FromType(Type type, bool resourceAllowed, List<Type> parentResourceTypes = null)
        {
            Schema schema;
            if (schemas.TryGetValue(type, out schema))
            {
                return schema;
            }
            schema = new Schema();
            schema.Name = type.Name.Split('.').Last();
            schema.Properties = new List<Property>();
            var list = new List<Type>(parentResourceTypes ?? new List<Type>());
            list.Add(type);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    if (resourceAllowed)
                    continue;
                    throw new IllegalNestingException();
                }
                Property property = new Property { Name = p.Name };
                var propertyType = p.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    property.IsCollection = true;
                    propertyType = propertyType.GetGenericArguments()[0];
                }
                if (list.Contains(propertyType))
                {
                    throw new RecursionException("Recursive schemas are not allowed.");
                }
                property.Type = GetType(propertyType);
                if (property.Type == BuiltInType.Object)
                {
                    property.Schema = FromType(propertyType, false, list);
                }
                schema.Properties.Add(property);
            }
            schemas.Add(type, schema);
            return schema;
        }


        private static BuiltInType GetType(Type type)
        {
            switch (type.ToString().Split('.').Last().ToLower())
            {
                case "string":
                    return BuiltInType.String;
                case "int32":
                    return BuiltInType.Int32;
                case "int64":
                    return BuiltInType.Int64;
                case "boolean":
                    return BuiltInType.Bool;
                default:
                    return BuiltInType.Object;
            }
        }
    }

    public enum BuiltInType { String, Int32, Int64, Bool, Object }

    public class Route
    {
        public Path Path { get; internal set; }
        public List<Operation> Operations { get; internal set; }
    }

    public class Path : List<PathComponent>
    {

    }

    public class PathComponent
    {
        public bool IsVariable { get; internal set; }
        public string Name { get; internal set; }
    }

    public class Operation
    {
        public Method Method { get; internal set; }
        public string Name { get; internal set; }
    }

    public enum Method { Get, Post, Put, Petch, Delete }
}
