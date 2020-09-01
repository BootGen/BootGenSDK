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
            c.Properties = new List<Property>{};
            classStore.Add(type, c);
            foreach (var p in type.GetProperties())
            {
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ResourceAttribute)))
                {
                    continue;
                }
                var propertyType = p.PropertyType;
                var property = GetProperty(propertyType);
                property.Name = p.Name;
                c.Properties.Add(property);
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ClientOnlyAttribute)))
                {
                    property.Location = Location.ClientOnly;
                }
                if (p.CustomAttributes.Any(d => d.AttributeType == typeof(ServerOnlyAttribute)))
                {
                    property.Location = Location.ServerOnly;
                }
            }

            if (type.CustomAttributes.Any( d => d.AttributeType == typeof(HasTimestampsAttribute))) {
                c.HasTimestamps = true;
                c.Properties.Add(new Property {
                    Name = "Created",
                    IsRequired = true,
                    BuiltInType = BuiltInType.DateTime
                });
                c.Properties.Add(new Property {
                    Name = "Updated",
                    IsRequired = true,
                    BuiltInType = BuiltInType.DateTime
                });
            }

            return c;
        }

        public Property GetProperty(Type propertyType)
        {
            var property = new Property();
            property.IsRequired = true;
            if (propertyType.IsGenericType)
            {
                Type genericType = propertyType.GetGenericTypeDefinition();
                if (genericType == typeof(List<>))
                {
                    property.IsCollection = true;
                    propertyType = propertyType.GetGenericArguments()[0];
                }
                if(genericType == typeof(Nullable<>))
                {
                    property.IsRequired = false;
                    propertyType = propertyType.GetGenericArguments()[0];
                }
            }
            property.BuiltInType = GetType(propertyType);
            if (property.BuiltInType == BuiltInType.Object)
            {
                property.Class = FromType(propertyType);
            } else if (property.BuiltInType == BuiltInType.Enum)
            {
                property.Enum = EnumFromType(propertyType);
            }
            return property;
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
