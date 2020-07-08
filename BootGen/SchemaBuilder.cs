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
            schema.Id = store.Schemas.Count;
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
                property.IsRequired = propertyType.IsValueType;
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
            } else if (typeDescription.BuiltInType == BuiltInType.Enum)
            {
                typeDescription.EnumSchema = EnumSchemaFromType(propertyType);
            }
            return typeDescription;
        }

        private EnumSchema EnumSchemaFromType(Type type)
        {
            EnumSchema schema;
            if (store.TryGetValue(type, out schema))
                return schema;

            schema = new EnumSchema();
            schema.Id = store.EnumSchemas.Count;
            schema.Name = type.Name.Split('.').Last();
            schema.Values = new List<string>();

            foreach (var value in Enum.GetValues(type))
            {
                schema.Values.Add(value.ToString());
            }

            store.Add(type, schema);

            return schema;
        }

        private static BuiltInType GetType(Type type)
        {
            if (type.IsEnum)
                return BuiltInType.Enum;
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
                case "datetime":
                    return BuiltInType.DateTime;
                default:
                    return BuiltInType.Object;
            }
        }
    }
}
