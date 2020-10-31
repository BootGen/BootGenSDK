using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

namespace BootGen
{
    internal class TypeBuilder
    {
        private readonly ClassCollection classStore;
        private readonly EnumCollection enumStore;
        private readonly bool persist;

        internal TypeBuilder(ClassCollection classStore, EnumCollection enumStore, bool persist)
        {
            this.classStore = classStore;
            this.enumStore = enumStore;
            this.persist = persist;
        }
        internal ClassModel FromType(Type type)
        {
            ClassModel c;
            if (classStore.TryGetValue(type, out c))
            {
                if (persist)
                    c.MakePersisted();
                return c;
            }
            return CreateClassForType(type);
        }

        private ClassModel CreateClassForType(Type type)
        {
            var c = new ClassModel();
            c.Name = type.Name.Split('.').Last();
            var cs = new CSharpCodeProvider();
            if (!cs.IsValidIdentifier(c.Name) || !cs.IsValidIdentifier(c.Name.Singular.ToCamelCase()) || !cs.IsValidIdentifier(c.Name.Singular.ToLower())) {
                throw new Exception($"\"{c.Name}\" can not be used as class name, because it is a reserved word.");
            }
            var pluralNameAttribute = type.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(PluralNameAttribute));
            c.Name.Plural = pluralNameAttribute?.ConstructorArguments?.FirstOrDefault().Value as string ?? c.Name + "s";
            c.Properties = new List<Property>{};
            classStore.Add(type, c);
            foreach (var p in type.GetProperties())
            {
                if (p.Name == "Id")
                    throw new Exception("Do not define an \"Id\" property explicitly!");
                var propertyType = p.PropertyType;
                var property = GetProperty<Property>(propertyType);
                property.Name = p.Name;
                var singularName = p.Get<SingularNameAttribute>()?.GetFirstParameter<string>();
                if (property.IsCollection)
                {
                    property.Noun = singularName ?? p.Name.Substring(0, p.Name.Length-1);
                    property.Noun.Plural = p.Name;
                } else {
                    property.Noun = singularName ?? p.Name;
                }
                c.Properties.Add(property);
                if (p.Has<ClientOnlyAttribute>() || p.Has<OneToManyAttribute>() || p.Has<ManyToManyAttribute>())
                {
                    property.Location = Location.ClientOnly;
                }
                property.IsManyToMany = p.Has<ManyToManyAttribute>();
                if (p.Has<ServerOnlyAttribute>())
                {
                    property.Location = Location.ServerOnly;
                }
            }

            if (type.Has<HasTimestampsAttribute>()) {
                c.HasTimestamps = true;
                c.Properties.Add(new Property {
                    Name = "Created",
                    IsRequired = true,
                    BuiltInType = BuiltInType.DateTime,
                    IsClientReadonly = true
                });
                c.Properties.Add(new Property {
                    Name = "Updated",
                    IsRequired = true,
                    BuiltInType = BuiltInType.DateTime,
                    IsClientReadonly = true
                });
            }

            if (persist)
                c.MakePersisted();
            return c;
        }

        internal T GetProperty<T>(Type propertyType) where T : TypeDescription, new()
        {
            var property = new T();
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
                case "guid":
                    return BuiltInType.Guid;
                default:
                    return BuiltInType.Object;
            }
        }
    }
}
