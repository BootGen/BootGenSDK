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
    public Dictionary<string, ClassSettings> ClassSettings { get; set; } = new Dictionary<string, ClassSettings>();
    public List<ClassModel> CommonClasses => Classes.Where(p => !p.IsServerOnly).ToList();
    public Func<BuiltInType, string> TypeToString { get; init; } = AspNetCoreGenerator.ToCSharpType;
    public Dictionary<WarningType, HashSet<string>> Warnings { get; } = new Dictionary<WarningType, HashSet<string>>();

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
            var model = Parse(property);
            if (model != null)
                model.IsRoot = true;
        }

        CheckForEmptyClasses();
        ApplySettings();
        AddRelationships();
    }

    private void CheckForEmptyClasses()
    {
        foreach (var c in Classes)
            if (c.Properties.Count == 1) {
                AddWarning(WarningType.EmptyType, c.Name);
            }
        Classes.RemoveAll(c => c.Properties.Count == 1);
        foreach (var c in Classes)
            c.Properties.RemoveAll(p =>p.Class?.Properties.Count == 1);
    }

    private void AddWarning(WarningType warningType, string name)
    {
        HashSet<string> names;
        if (!Warnings.TryGetValue(warningType, out names)) {
            names = new HashSet<string>();
            Warnings.Add(warningType, names);
        }
        names.Add(name);
    }

    public void LoadRootObject(string name, JObject jObject)
    {
        var property = new JProperty(name, jObject);
        var model = Parse(property);
        if (model != null) {
            model.IsRoot = true;
            foreach (var prop in model.Properties) {
                if (!prop.IsKey && prop.BuiltInType != BuiltInType.Object) {
                    AddWarning(WarningType.PrimitiveRoot, prop.Name);
                }
            }
        }
        CheckForEmptyClasses();
        AddRelationships();
    }

    private void ApplySettings()
    {
        foreach (var cl in new List<ClassModel>(Classes)) {
            if (!ClassSettings.TryGetValue(cl.Name, out var classSettings))
                continue;
            if (classSettings.HasTimestamps)
            {
                cl.HasTimestamps = true;
                cl.Properties.Insert(1, new Property
                {
                    Name = "Created",
                    BuiltInType = BuiltInType.DateTime
                });
                cl.Properties.Insert(2, new Property
                {
                    Name = "Updated",
                    BuiltInType = BuiltInType.DateTime
                });
            }
            foreach (var property in new List<Property>(cl.Properties))
            {
                if (!classSettings.PropertySettings.TryGetValue(property.Name, out var propertySettings))
                    continue;
                if (propertySettings.IsHidden) {
                    cl.Properties.Remove(property);
                    continue;
                }
                property.IsManyToMany = propertySettings.IsManyToMany;
                property.VisibleName = propertySettings.VisibleName ?? property.Name;
                property.IsReadOnly = propertySettings.IsReadOnly;
                if (!string.IsNullOrEmpty(propertySettings.ClassName))
                {
                    var to = Classes.First(c => c.Name == propertySettings.ClassName);
                    Merge(to, property.Class);
                }
            }
        }
    }

    private void Merge(ClassModel to, ClassModel from)
    {
        foreach(var property in from.Properties) {
            if (to.Properties.Any(p => p.Name == property.Name))
                continue;
            to.Properties.Add(property);
        }
        Classes.Remove(from);
        foreach (var cl in Classes)
            foreach(var property in cl.Properties)
                if (property.Class == from)
                    property.Class = to;
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

    private ClassModel Parse(JProperty property)
    {
        if (!provider.IsValidIdentifier(property.Name))
            throw new FormatException($"\"{property.Name}\" is not a valid identifier.");
        var pluralizer = new Pluralizer();
        Noun className;
        if (property.Value.Type == JTokenType.Array)
        {
            var suggestedName = pluralizer.Pluralize(pluralizer.Singularize(property.Name));
            if (property.Name.ToLower() != suggestedName.ToLower())
                throw new NamingException($"Array names must be plural nouns. Did you mean \"{suggestedName}\"?", suggestedName, property.Name, true);
            className = pluralizer.Singularize(property.Name).Capitalize();
            className.Plural = property.Name.Capitalize();
        }
        else
        {
            var suggestedName = pluralizer.Singularize(pluralizer.Pluralize(property.Name));
            if (property.Name.ToLower() != suggestedName.ToLower())
                throw new NamingException($"Object names must be singular nouns. The property name \"{property.Name}\" does not seam to be singular. Did you mean \"{suggestedName}\"?", suggestedName, property.Name, false);
            className = property.Name.Capitalize();
            className.Plural = pluralizer.Pluralize(property.Name).Capitalize();
        }
        
        if (property.Value.Type == JTokenType.Array) {
                var data = property.Value as JArray;
                foreach (JToken item in data)
                {
                    if (item.Type == JTokenType.Array) {
                        AddWarning(WarningType.NestedArray, property.Name);
                        return null;
                    } else if (item.Type != JTokenType.Object) {
                        AddWarning(WarningType.PrimitiveArrayElement, property.Name);
                        return null;
                    }
                }
        }
        ClassModel result = GetClassModel(className);
        if (property.Value.Type == JTokenType.Array)
            result.ReferredPlural = true;
        if (property.Value.Type == JTokenType.Object)
            result.ReferredSingle = true;
        switch (property.Value.Type)
        {
            case JTokenType.Array:
                var data = property.Value as JArray;
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
            if (!provider.IsValidIdentifier(property.Name))
                throw new FormatException($"\"{property.Name}\" is not a valid identifier.");
            var propertyName = property.Name.Capitalize();
            var prop = model.Properties.FirstOrDefault(p => p.Name == propertyName);

            if (property.Value.Type == JTokenType.Null)
                continue;

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
                if (builtInType == BuiltInType.Object) {
                    Parse(property);
                }
                continue;
            }

            prop = new Property
            {
                Name = propertyName,
                BuiltInType = builtInType,
                IsCollection = isCollection
            };
            model.Properties.Add(prop);
            if (prop.IsCollection)
            {
                prop.Noun = pluralizer.Singularize(propertyName);
                prop.Noun.Plural = propertyName;
            }
            if (prop.BuiltInType == BuiltInType.Object)
            {
                prop.Class = Parse(property);
                if (prop.Class == null) {
                    model.Properties.Remove(prop);
                    continue;
                }
                if (prop.IsCollection)
                    prop.IsServerOnly = true;
            }
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
            if (!c.Properties.Any(p => p.Name == property.Name + ClassModel.IdName))
            {
                c.Properties.Insert(propertyIdx + 1, new Property
                {
                    Name = property.Name + ClassModel.IdName,
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

        if (!child.Properties.Any(p => p.Name == parent.Name + ClassModel.IdName))
        {

            var idProperty = new Property
            {
                Name = parent.Name + ClassModel.IdName,
                BuiltInType = BuiltInType.Int,
                IsKey = true
            };
            child.Properties.Add(idProperty);
        }
    }
}
