using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pluralize.NET;

namespace BootGen;

public class DataModel
{
    public List<ClassModel> Classes { get; } = new List<ClassModel>();
    public List<ClassModel> CommonClasses => Classes.Where(p => !p.IsServerOnly).ToList();

    public Func<BuiltInType, string> TypeToString { get; init; } = AspNetCoreGenerator.ToCSharpType;

    private CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");

    public void AddClass(ClassModel c)
    {
        c.Id = Classes.Count;
        Classes.Add(c);
    }
    public void Load(JObject jObject)
    {
        foreach (var property in jObject.Properties())
        {
            var model = Parse(property, out var _);
            model.IsRoot = true;
        }

        CheckForEmptyClasses();
        AddRelationships();
    }

    private void CheckForEmptyClasses()
    {
        foreach (var c in Classes)
            if (c.Properties.Count == 1)
                throw new FormatException($"Empty types are not supported. The folowing class has no properties: \"{c.Name}\"");
    }

    public void LoadRootObject(string name, JObject jObject)
    {
        var property = new JProperty(name, jObject);
        var model = Parse(property, out var _);
        model.IsRoot = true;
        CheckForEmptyClasses();
        AddRelationships();
    }

    private void AddRelationships()
    {
        foreach (var c in Classes)
        {
            var properties = new List<Property>(c.Properties);
            foreach (var p in properties)
                if (p.IsCollection && p.BuiltInType == BuiltInType.Object && !p.IsManyToMany)
                    AddOneToManyParentReference(c, p);
        }

        foreach (var c in Classes)
        {
            AddManyToManyMirrorProperties(c);
            AddManyToOneParentReference(c);
        }
    }

    private ClassModel Parse(JProperty property, out bool manyToMany)
    {
        if (!provider.IsValidIdentifier(property.Name))
            throw new FormatException($"\"{property.Name}\" is not a valid identifier.");
        var pluralizer = new Pluralizer();
        Noun className;
        bool hasTimestamps = false;
        manyToMany = false;
        if (property.Value.Type == JTokenType.Array)
        {
            var suggestedName = pluralizer.Pluralize(pluralizer.Singularize(property.Name));
            if (property.Name.ToLower() != suggestedName.ToLower())
                throw new NamingException($"Array names must be plural nouns. The property name \"{property.Name}\" does not seam to be plural. Did you mean \"{suggestedName}\"?", suggestedName, property.Name);
            className = pluralizer.Singularize(property.Name).Capitalize();
            className.Plural = property.Name.Capitalize();
            var data = property.Value as JArray;
            foreach (JToken item in data)
            {
                if (item.Type != JTokenType.Comment)
                    continue;
                string comment = item.Value<string>().Trim();
                if (comment == "manyToMany")
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
                    string name = comment.Split(":").Last().Trim();
                    if (!provider.IsValidIdentifier(name))
                    {
                        throw new Exception($"Invalid class name: {name}");
                    }
                    className = name.Capitalize();
                    className.Plural = pluralizer.Pluralize(property.Name).Capitalize();
                    continue;
                }
                throw new Exception($"Unrecognised annotation: {comment}");
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
                    } else if (item.Type == JTokenType.Comment) {
                        continue;
                    } else if (item.Type == JTokenType.Array) {
                        throw new FormatException("Nested arrays are not supported.");
                    } else {
                        throw new FormatException("Primitive types as array elements are not supported.");
                    }
                }
                break;
            case JTokenType.Object:
                ExtendModel(result, property.Value as JObject);
                break;
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
        var pluralizer = new Pluralizer();
        foreach (var property in item.Properties())
        {
            var propertyName = property.Name.Capitalize();
            var prop = model.Properties.FirstOrDefault(p => p.Name == propertyName);

            BuiltInType builtInType = ConvertType(property.Value.Type);
            bool isCollection = property.Value.Type == JTokenType.Array;
            if (prop != null)
            {
                if (isCollection != prop.IsCollection)
                {
                    string type1;
                    string type2;
                    if (isCollection && !prop.IsCollection)
                    {
                        type1 = TypeToString(prop.BuiltInType);
                        type2 = "array";
                    }
                    else
                    {
                        type1 = "array";
                        type2 = TypeToString(prop.BuiltInType);
                    }
                    throw GetFormatException(model, propertyName, type1, type2);
                }
                if (prop.BuiltInType != builtInType) {
                    if (prop.BuiltInType == BuiltInType.Float && builtInType == BuiltInType.Int) {
                        continue;
                    }
                    if (prop.BuiltInType == BuiltInType.Int && builtInType == BuiltInType.Float) {
                        prop.BuiltInType = BuiltInType.Float;
                        continue;
                    }
                    throw GetFormatException(model, propertyName, TypeToString(prop.BuiltInType), TypeToString(builtInType));
                }
                continue;
            }

            prop = new Property
            {
                Name = propertyName,
                BuiltInType = builtInType,
                IsCollection = isCollection
            };
            if (prop.IsCollection)
            {
                prop.Noun = pluralizer.Singularize(propertyName);
                prop.Noun.Plural = propertyName;
            }
            if (prop.BuiltInType == BuiltInType.Object)
            {
                prop.Class = Parse(property, out bool m2m);
                prop.IsManyToMany = m2m;
                if (prop.IsCollection)
                    prop.IsServerOnly = true;
            }
            model.Properties.Add(prop);
        }
    }

    private static FormatException GetFormatException(ClassModel model, string propertyName, string type1, string type2)
    {
        return new FormatException($"The \"{propertyName}\" property of the class \"{model.Name}\" has inconsistent type: {type1} and  {type2} is both used.");
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
            default:
                return BuiltInType.Object;
        }
    }
    private void AddManyToManyMirrorProperties(ClassModel c)
    {
        var pluralizer = new Pluralizer();
        var properties = new List<Property>(c.Properties);
        foreach (var property in properties)
        {
            if (!property.IsManyToMany)
                continue;

            Property referenceProperty = property.Class.Properties.FirstOrDefault(p => p.Class == c);
            if (referenceProperty == null)
            {
                referenceProperty = new Property
                {
                    Name = c.Name.Plural,
                    BuiltInType = BuiltInType.Object,
                    Class = c,
                    IsCollection = true,
                    IsManyToMany = true,
                    IsServerOnly = true,
                    Noun = c.Name
                };
                property.Class.Properties.Add(referenceProperty);
            }
            referenceProperty.MirrorProperty = property;
            property.MirrorProperty = referenceProperty;
        }
    }

    public void AddManyToOneParentReference(ClassModel c)
    {
        var properties = new List<Property>(c.Properties);
        var propertyIdx = -1;
        foreach (var property in properties)
        {
            propertyIdx += 1;
            if (property.Class == null || property.IsCollection)
                continue;
            if (!c.Properties.Any(p => p.Name == property.Name + "Id"))
            {
                c.Properties.Insert(propertyIdx + 1, new Property
                {
                    Name = property.Name + "Id",
                    BuiltInType = BuiltInType.Int,
                    IsKey = true
                });
                property.IsServerOnly = true;
                propertyIdx += 1;
            }
        }
    }


    private static void AddOneToManyParentReference(ClassModel parent, Property property)
    {
        var referenceProperty = new Property
        {
            Name = parent.Name,
            BuiltInType = BuiltInType.Object,
            Class = parent,
            IsServerOnly = true,
            IsParentReference = true,
            MirrorProperty = property
        };
        var child = property.Class;
        child.Properties.Add(referenceProperty);

        if (!child.Properties.Any(p => p.Name == parent.Name + "Id"))
        {

            var idProperty = new Property
            {
                Name = parent.Name + "Id",
                BuiltInType = BuiltInType.Int,
                IsKey = true
            };
            child.Properties.Add(idProperty);
        }
    }
}
