using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pluralize.NET;

namespace BootGen
{
    public class DataModel
    {
        public List<ClassModel> Classes => ClassCollection.Classes;
        public List<ClassModel> CommonClasses => Classes.Where(p => p.Location == PropertyType.Normal).ToList();
        internal ClassCollection ClassCollection { get; }

        public DataModel()
        {
            ClassCollection = new ClassCollection();
        }

        public void AddClass(ClassModel c)
        {
            ClassCollection.Add(c);
        }
        public void Load(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                Parse(property, out var _);
            }

            foreach (var c in Classes)
            {
                var properties = new List<Property>(c.Properties);
                foreach (var p in properties)
                    if (p.IsCollection && p.BuiltInType == BuiltInType.Object && !p.IsManyToMany)
                        AddOneToManyParentReference(c, p.Class);
            }

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
            bool hasTimestamps = false;
            manyToMany = false;
            if (property.Value.Type == JTokenType.Array)
            {
                className = pluralizer.Singularize(property.Name).Capitalize();
                className.Plural = property.Name.Capitalize();
                var data = property.Value as JArray;
                foreach (JToken item in data)
                {
                    if (item.Type != JTokenType.Comment)
                        continue;
                    string comment = item.Value<string>().Trim();
                    if (comment == "many-to-many")
                    {
                        manyToMany = true;
                        continue;
                    }
                    if (comment == "timestamps")
                    {
                        hasTimestamps = true;
                        continue;
                    }
                    if (comment.StartsWith("class:"))
                    {
                        string name = comment.Split(":").Last();
                        var provider = CodeDomProvider.CreateProvider("C#");
                        if (provider.IsValidIdentifier(name))
                        {
                            className = name.Capitalize();
                            className.Plural = pluralizer.Pluralize(property.Name).Capitalize();
                            continue;
                        }
                    }
                    throw new Exception($"Unrecognised hint: {comment}");
                }
            }
            else
            {
                className = property.Name.Capitalize();
                className.Plural = pluralizer.Pluralize(property.Name).Capitalize();
            }
            ClassModel result = GetClassModel(className);
            if (hasTimestamps && !result.HasTimestamps)
            {
                result.HasTimestamps = true;
                result.Properties.Add(new Property
                {
                    Name = "Created",
                    BuiltInType = BuiltInType.DateTime
                });
                result.Properties.Add(new Property
                {
                    Name = "Updated",
                    BuiltInType = BuiltInType.DateTime
                });
            }
            switch (property.Value.Type)
            {
                case JTokenType.Array:
                    var data = property.Value as JArray;
                    var comments = new List<JToken>();
                    foreach (JToken item in data)
                    {
                        if (item.Type == JTokenType.Object)
                        {
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

        private ClassModel GetClassModel(Noun className)
        {
            var c = Classes.FirstOrDefault(c => c.Name.Singular == className);
            if (c == null)
            {
                c = new ClassModel(className);
                AddClass(c);
            }
            return c;
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
                    if (prop.BuiltInType == BuiltInType.Object)
                    {
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
                    return BuiltInType.Int;
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
            var properties = new List<Property>(c.Properties);
            foreach (var property in properties)
            {
                if (property.Class == null || !property.IsCollection || property.MirrorProperty != null)
                    continue;

                Property referenceProperty = property.Class.Properties.FirstOrDefault(p => p.Class == c);
                if (referenceProperty == null)
                {
                    referenceProperty = new Property
                    {
                        Name = property.IsManyToMany ? c.Name.Plural : c.Name.Singular,
                        BuiltInType = BuiltInType.Object,
                        Class = c,
                        IsCollection = property.IsManyToMany,
                        IsManyToMany = property.IsManyToMany,
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
                        BuiltInType = BuiltInType.Int
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
                            BuiltInType = BuiltInType.Int,
                            PropertyType = PropertyType.Normal
                        });
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
                    BuiltInType = BuiltInType.Int,
                    IsCollection = false
                };
                child.Properties.Add(property);
            }
        }


    }
}