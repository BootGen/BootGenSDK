using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class SchemaBuilder
    {
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
            return CreateSchemaForType(type);
        }

        private Schema CreateSchemaForType(Type type)
        {
            Schema schema = new Schema();
            schema.Name = type.Name.Split('.').Last();
            schema.Properties = new List<Property>();
            store.Add(type, schema);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    continue;
                }
                var propertyType = p.PropertyType;
                var property = GetTypeDescription<Property>(propertyType);
                property.Name = p.Name;
                property.Required = propertyType.IsValueType;
                schema.Properties.Add(property);
                if (property.Name.ToLower() == "id")
                {
                    schema.IdProperty = property;
                }
            }

            return schema;
        }

        public T GetTypeDescription<T>(Type propertyType) where T : TypeDescription, new()
        {
            T typeDescription = new T();
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                typeDescription.IsCollection = true;
                propertyType = propertyType.GetGenericArguments()[0];
            }
            typeDescription.BuiltInType = GetType(propertyType);
            if (typeDescription.BuiltInType == BuiltInType.Object)
            {
                typeDescription.Schema = FromType(propertyType);
            }
            return typeDescription;
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
