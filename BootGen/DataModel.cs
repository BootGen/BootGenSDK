using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pluralize.NET;

namespace BootGen
{
    public class DataModel
    {
        public List<ClassModel> Classes => ClassCollection.Classes;
        public List<EnumModel> Enums => EnumCollection.Enums;
        public List<ClassModel> CommonClasses => Classes.Where(p => p.Location == PropertyType.Normal).ToList();
        internal ClassCollection ClassCollection { get; }
        internal EnumCollection EnumCollection { get; }

        public DataModel()
        {
            ClassCollection = new ClassCollection();
            EnumCollection = new EnumCollection();
        }

        public void AddClass(ClassModel c)
        {
            ClassCollection.Add(c);
        }
        public void AddEnum(EnumModel e)
        {
            EnumCollection.Add(e);
        }
        public void Load(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                Parse(property, out var _);
            }

            foreach (var c in Classes)
                foreach (var p in c.Properties)
                    if (p.IsCollection && p.BuiltInType == BuiltInType.Object && !p.IsManyToMany)
                        AddOneToManyParentReference(c, p.Class);

            foreach (var c in Classes)
            {
                if (!c.RelationsAreSetUp)
                {
                    AddEfRelationsParentToChild(c);
                }
                AddEfRelationsChildToParent(c);
            }
        }


        private ClassModel Parse(JProperty property, out bool manyToMany)
        {
            var pluralizer = new Pluralizer();
            Noun className;
            if (property.Value.Type == JTokenType.Array)
            {
                className = pluralizer.Singularize(property.Name).Capitalize();
                className.Plural = property.Name.Capitalize();
            } else {
                className = property.Name.Capitalize();
                className.Plural = pluralizer.Pluralize(property.Name).Capitalize();
            }
            ClassModel result = Classes.FirstOrDefault(c => c.Name.Singular == className);
            if (result == null)
            {
                result = new ClassModel(className);
                AddClass(result);
            }
            manyToMany = false;
            switch (property.Value.Type)
            {
                case JTokenType.Array:
                    var data = property.Value as JArray;
                    var comments = new List<JToken>();
                    foreach (JToken item in data)
                    {
                        if (item.Type == JTokenType.Comment) {
                            if (item.Value<string>() == "many-to-many") {
                                manyToMany = true;
                            }
                        }
                        if (item.Type == JTokenType.Object) {
                            ExtendModel(result, (JObject)item);
                        }
                    }
                break;
                case JTokenType.Object:
                    ExtendModel(result, property.Value as JObject);
                break;
                default:
                return null;
            }
            return result;
        }

        private void ExtendModel(ClassModel model, JObject item)
        {
            foreach (var property in item.Properties())
            {
                var propertyName = property.Name.Capitalize();
                if (model.Properties.All(p => p.Name != propertyName))
                {
                    var prop = new Property
                    {
                        Name = propertyName,
                        BuiltInType = ConvertType(property.Value.Type),
                        IsCollection = property.Value.Type == JTokenType.Array
                    };
                    if (prop.BuiltInType == BuiltInType.Object) {
                        prop.Class = Parse(property, out bool m2m);
                        prop.IsManyToMany = m2m;
                        if (prop.IsCollection)
                            prop.PropertyType = PropertyType.ServerOnly;
                    }
                    model.Properties.Add(prop);
                }
            }
        }

        private BuiltInType ConvertType(JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Boolean:
                    return BuiltInType.Bool;
                case JTokenType.String:
                    return BuiltInType.String;
                case JTokenType.Date:
                    return BuiltInType.DateTime;
                case JTokenType.Integer:
                    return BuiltInType.Int32;
                case JTokenType.Guid:
                    return BuiltInType.Guid;
                case JTokenType.Float:
                    return BuiltInType.Float;
                case JTokenType.Array:
                    return BuiltInType.Object;
                case JTokenType.Object:
                    return BuiltInType.Object;
            }
            return BuiltInType.Object;
        }
        private void AddEfRelationsParentToChild(ClassModel c)
        {
            c.RelationsAreSetUp = true;
            foreach (var property in c.Properties)
            {
                if (property.Class == null || !property.IsCollection || property.MirrorProperty != null || property.PropertyType == PropertyType.Virtual)
                    continue;

                Property referenceProperty = property.Class.Properties.FirstOrDefault(p => p.Name == c.Name);
                if (referenceProperty == null)
                {
                    referenceProperty = new Property
                    {
                        Name = property.IsManyToMany ? c.Name.Plural : c.Name.Singular,
                        BuiltInType = BuiltInType.Object,
                        Class = c,
                        IsCollection = property.IsManyToMany,
                        IsManyToMany = property.IsManyToMany,
                        IsRequired = true,
                        PropertyType = PropertyType.ServerOnly,
                        IsParentReference = true
                    };
                    property.Class.Properties.Add(referenceProperty);
                }
                referenceProperty.MirrorProperty = property;
                property.MirrorProperty = referenceProperty;

                if (!property.IsManyToMany && !property.Class.Properties.Any(p => p.Name == c.Name + "Id"))
                    property.Class.Properties.Add(new Property
                    {
                        Name = c.Name + "Id",
                        BuiltInType = BuiltInType.Int32,
                        IsRequired = true
                    });
                AddEfRelationsParentToChild(property.Class);
            }
        }

        public void AddEfRelationsChildToParent(ClassModel c)
        {
            var properties = new List<Property>(c.Properties);
            var propertyIdx = -1;
            foreach (var property in properties)
            {
                propertyIdx += 1;
                if (property.Class == null)
                    continue;
                if (!property.IsCollection && property.Class != null)
                {
                    if (!c.Properties.Any(p => p.Name == property.Name + "Id"))
                    {
                        c.Properties.Insert(propertyIdx + 1, new Property
                        {
                            Name = property.Name + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = property.Class.IsResource,
                            PropertyType = property.Class.IsResource ? PropertyType.Normal : PropertyType.ServerOnly
                        });
                        if (property.Class.IsResource)
                            property.PropertyType = PropertyType.ServerOnly;
                        propertyIdx += 1;
                        AddEfRelationsChildToParent(property.Class);
                    }
                }
            }
        }


        private static void AddOneToManyParentReference(ClassModel parent, ClassModel child)
        {
            if (!child.Properties.Any(p => p.Name == parent.Name))
            {
                var referenceProperty = new Property
                {
                    Name = parent.Name,
                    BuiltInType = BuiltInType.Object,
                    Class = parent,
                    IsCollection = false,
                    IsRequired = true,
                    PropertyType = PropertyType.ServerOnly,
                    IsParentReference = true
                };
                child.Properties.Add(referenceProperty);
            }
            else
            {
                var referenceProperty = child.Properties.First(p => p.Name == parent.Name);
                referenceProperty.IsParentReference = true;
            }

            if (!child.Properties.Any(p => p.Name == parent.Name + "Id"))
            {
                Property property = new Property
                {
                    Name = parent.Name + "Id",
                    BuiltInType = BuiltInType.Int32,
                    IsCollection = false,
                    IsRequired = true
                };
                child.Properties.Add(property);
            }
        }


    }
}