using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pluralize.NET;

namespace BootGen.Core;

public class DataModel
{
    public List<Class> Classes { get; } = new List<Class>();
    public List<Class> CommonClasses => Classes.Where(p => !p.IsServerOnly).ToList();
    public Func<BuiltInType, string> TypeToString { get; init; } = CSharpGenerator.ToCSharpType;
    public Dictionary<WarningType, HashSet<string>> Warnings { get; } = new Dictionary<WarningType, HashSet<string>>();
    public bool GenerateIds { get; set; } = true;

    private CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");

    public void AddClass(Class @class)
    {
        @class.Id = Classes.Count;
        Classes.Add(@class);
    }
    public void Load(JObject jObject, List<ClassSettings> settings = null)
    {
        foreach (var property in jObject.Properties())
            Parse(property);

        CheckForEmptyClasses();
        if (settings != null)
            ApplySettings(settings);
        AddRelationships();
    }

    private void CheckForEmptyClasses()
    {
        foreach (var c in Classes)
            if (c.IsEmpty) {
                AddWarning(WarningType.EmptyType, c.Name);
            }
        Classes.RemoveAll(c => c.IsEmpty);
        foreach (var c in Classes)
            c.AllProperties.RemoveAll(p =>p.Class?.IsEmpty == true);
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

    public void LoadRootObject(string name, JObject jObject, List<ClassSettings> settings = null)
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
        if (settings != null)
            ApplySettings(settings);
        AddRelationships();
    }

    private void ApplySettings(List<ClassSettings> settings)
    {
        var classSettingsDict = settings.ToDictionary(s => s.Name);
        foreach (var cl in new List<Class>(Classes)) {
            if (!classSettingsDict.TryGetValue(cl.Name, out var classSettings))
                continue;
            if (classSettings.HasTimestamps)
            {
                cl.HasTimestamps = true;
                cl.AllProperties.Insert(1, new Property
                {
                    Name = "Created",
                    BuiltInType = BuiltInType.DateTime
                });
                cl.AllProperties.Insert(2, new Property
                {
                    Name = "Updated",
                    BuiltInType = BuiltInType.DateTime
                });
            }
            var propSettingsDict = classSettings.PropertySettings.ToDictionary(s => s.Name);
            foreach (var property in new List<Property>(cl.SettingsProperties))
            {
                if (!propSettingsDict.TryGetValue(property.Name, out var propertySettings))
                    continue;
                property.IsManyToMany = propertySettings.IsManyToMany == true;
                if (propertySettings.VisibleName != null)
                    property.VisibleName = propertySettings.VisibleName;
                property.IsReadOnly = propertySettings.IsReadOnly == true;
                property.IsHidden = propertySettings.IsHidden;
                if (!string.IsNullOrEmpty(propertySettings.ClassName))
                {
                    var to = Classes.First(c => c.Name == propertySettings.ClassName);
                    Merge(to, property.Class);
                }
            }
        }
    }

    public List<ClassSettings> GetSettings()
    {
        var result = new List<ClassSettings>();

        foreach (var cl in Classes)
        {
            if (cl.IsRoot)
                continue;
            var classSettings = new ClassSettings{
                Name = cl.Name.Singular,
                HasTimestamps = cl.HasTimestamps,
                PropertySettings = new List<PropertySettings>()
            };
            result.Add(classSettings);
            foreach (var property in cl.SettingsProperties)
            {
                var propertySettings = new PropertySettings {
                    Name = property.Name,
                    VisibleName = property.VisibleName,
                    IsManyToMany = property.IsCollection ? property.IsManyToMany : null,
                    IsReadOnly = property.BuiltInType == BuiltInType.Object ? null : property.IsReadOnly,
                    IsHidden = property.IsHidden
                };
                if (property.BuiltInType == BuiltInType.Object)
                    if (!property.Class.Name.Equals(property.Noun))
                        propertySettings.ClassName = property.Class.Name.Singular;
                classSettings.PropertySettings.Add(propertySettings);
            }
        }
        return result;
    }

    private void Merge(Class to, Class from)
    {
        foreach(var property in from.Properties) {
            if (to.AllProperties.Any(p => p.Name == property.Name))
                continue;
            to.AllProperties.Add(property);
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
            if (GenerateIds)
                AddManyToOneParentReference(c);
        }
    }

    private Class Parse(JProperty property)
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
        Class result = GetClassModel(className);
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
                        ExtendClass(result, (JObject)item);
                    }
                }
                break;
            case JTokenType.Object:
                ExtendClass(result, property.Value as JObject);
                break;
        }
        return result;
    }

    private Class GetClassModel(Noun className)
    {
        var @class = Classes.FirstOrDefault(c => c.Name.Singular == className);
        if (@class == null)
        {
            @class = new Class(className);
            if (GenerateIds)
                @class.CreateId();
            AddClass(@class);
        }
        return @class;
    }

    private void ExtendClass(Class @class, JObject item)
    {
        var pluralizer = new Pluralizer();
        foreach (var property in item.Properties())
        {
            if (!provider.IsValidIdentifier(property.Name))
                throw new FormatException($"\"{property.Name}\" is not a valid identifier.");
            var propertyName = property.Name.Capitalize();
            var prop = @class.Properties.FirstOrDefault(p => p.Name == propertyName);

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
                    throw GetFormatException(@class, propertyName, type1, type2);
                }
                if (prop.BuiltInType != builtInType) {
                    if (prop.BuiltInType == BuiltInType.Float && builtInType == BuiltInType.Int) {
                        continue;
                    }
                    if (prop.BuiltInType == BuiltInType.Int && builtInType == BuiltInType.Float) {
                        prop.BuiltInType = BuiltInType.Float;
                        continue;
                    }
                    throw GetFormatException(@class, propertyName, TypeToString(prop.BuiltInType), TypeToString(builtInType));
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
            @class.AllProperties.Add(prop);
            if (prop.BuiltInType == BuiltInType.Object)
            {
                if (prop.IsCollection)
                {
                    prop.Noun = pluralizer.Singularize(propertyName);
                    prop.Noun.Plural = propertyName;
                } else {
                    prop.Noun = propertyName;
                    prop.Noun.Plural = pluralizer.Pluralize(propertyName);
                }
                prop.Class = Parse(property);
                if (prop.Class == null) {
                    @class.AllProperties.Remove(prop);
                    continue;
                }
                if (prop.IsCollection)
                    prop.IsServerOnly = true;
            }
        }
    }

    private static FormatException GetFormatException(Class @class, string propertyName, string type1, string type2)
    {
        return new FormatException($"The \"{propertyName}\" property of the class \"{@class.Name}\" has inconsistent type: {type1} and  {type2} is both used.");
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
    private void AddManyToManyMirrorProperties(Class @class)
    {
        var pluralizer = new Pluralizer();
        var properties = new List<Property>(@class.Properties);
        foreach (var property in properties)
        {
            if (!property.IsManyToMany)
                continue;

            Property referenceProperty = property.Class.Properties.FirstOrDefault(p => p.Class == @class);
            if (referenceProperty == null)
            {
                referenceProperty = new Property
                {
                    Name = @class.Name.Plural,
                    BuiltInType = BuiltInType.Object,
                    Class = @class,
                    IsCollection = true,
                    IsManyToMany = true,
                    IsServerOnly = true,
                    Noun = @class.Name
                };
                property.Class.AllProperties.Add(referenceProperty);
            }
            referenceProperty.MirrorProperty = property;
            property.MirrorProperty = referenceProperty;
        }
    }

    public void AddManyToOneParentReference(Class @class)
    {
        var properties = new List<Property>(@class.Properties);
        var propertyIdx = -1;
        foreach (var property in properties)
        {
            propertyIdx += 1;
            if (property.Class == null || property.IsCollection)
                continue;
            if (!@class.Properties.Any(p => p.Name == property.Name + Class.IdName))
            {
                @class.AllProperties.Insert(propertyIdx + 1, new Property
                {
                    Name = property.Name + Class.IdName,
                    BuiltInType = BuiltInType.Int,
                    IsKey = true
                });
                property.IsServerOnly = true;
                propertyIdx += 1;
            }
        }
    }


    private  void AddOneToManyParentReference(Class parent, Property property)
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
        child.AllProperties.Add(referenceProperty);

        if (GenerateIds && !child.Properties.Any(p => p.Name == parent.Name + Class.IdName))
        {

            var idProperty = new Property
            {
                Name = parent.Name + Class.IdName,
                BuiltInType = BuiltInType.Int,
                IsKey = true
            };
            child.AllProperties.Add(idProperty);
        }
    }
}
