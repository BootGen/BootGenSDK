using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class TypeBuilder
    {
        private readonly ClassStore classStore;
        private readonly EnumStore enumStore;

        internal TypeBuilder(ClassStore classStore, EnumStore enumStore)
        {
            this.classStore = classStore;
            this.enumStore = enumStore;
        }
        internal ClassModel FromType(Type type)
        {
            ClassModel c;
            if (classStore.TryGetValue(type, out c))
            {
                return c;
            }
            return CreateClassForType(type);
        }

        private ClassModel CreateClassForType(Type type)
        {
            var c = new ClassModel();
            c.Name = type.Name.Split('.').Last();
            c.Properties = new List<Property>();
            classStore.Add(type, c);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute) || d.AttributeType == typeof(WithPivotAttribute)))
                {
                    continue;
                }
                var propertyType = p.PropertyType;
                var property = GetTypeDescription<Property>(propertyType);
                property.Name = p.Name;
                property.IsRequired = propertyType.IsValueType && !propertyType.IsGenericType;
                c.Properties.Add(property);
                if (property.Name.ToLower() == "id")
                {
                    c.IdProperty = property;
                }
            }

            return c;
        }

        public T GetTypeDescription<T>(Type propertyType) where T : TypeDescription, new()
        {
            T typeDescription = new T();
            if (propertyType.IsGenericType)
            {
                Type genericType = propertyType.GetGenericTypeDefinition();
                if (genericType == typeof(List<>))
                {
                    typeDescription.IsCollection = true;
                    propertyType = propertyType.GetGenericArguments()[0];
                }
                if(genericType == typeof(Nullable<>))
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                }
            }
            typeDescription.BuiltInType = GetType(propertyType);
            if (typeDescription.BuiltInType == BuiltInType.Object)
            {
                typeDescription.ClassModel = FromType(propertyType);
            } else if (typeDescription.BuiltInType == BuiltInType.Enum)
            {
                typeDescription.EnumModel = EnumFromType(propertyType);
            }
            return typeDescription;
        }

        private EnumModel EnumFromType(Type type)
        {
            EnumModel e;
            if (enumStore.TryGetValue(type, out e))
                return e;

            e = new EnumModel();
            e.Id = enumStore.Enums.Count;
            e.Name = type.Name.Split('.').Last();
            e.Values = new List<string>();

            foreach (var value in System.Enum.GetValues(type))
            {
                e.Values.Add(value.ToString());
            }

            enumStore.Add(type, e);

            return e;
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
