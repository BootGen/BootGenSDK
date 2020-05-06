using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class SchemaBuilder
    {

        private readonly Stack<Type> typeStack = new Stack<Type>();
        private readonly SchemaStore store;

        internal SchemaBuilder(SchemaStore store)
        {
            this.store = store;
        }
        internal Schema FromType(Type type)
        {
            Schema schema;
            if (store.TryGetValue(type, out schema))
            {
                return schema;
            }
            schema = CreateSchemaForType(type);
            store.Add(type, schema);
            return schema;
        }

        private Schema CreateSchemaForType(Type type)
        {
            Schema schema = new Schema();
            schema.Name = type.Name.Split('.').Last();
            schema.Properties = new List<Property>();
            typeStack.Push(type);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    continue;
                }
                Property property = new Property { Name = p.Name };
                var propertyType = p.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    property.IsCollection = true;
                    propertyType = propertyType.GetGenericArguments()[0];
                }
                if (typeStack.Contains(propertyType))
                {
                    throw new RecursionException("Recursive schemas are not allowed.");
                }
                property.Type = GetType(propertyType);
                if (property.Type == BuiltInType.Object)
                {
                    property.Schema = FromType(propertyType);
                }
                schema.Properties.Add(property);
            }
            typeStack.Pop();

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
}
