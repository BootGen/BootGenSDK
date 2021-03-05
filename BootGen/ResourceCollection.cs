using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Pluralize.NET;

namespace BootGen
{
    public class ResourceCollection
    {
        public List<RootResource> RootResources { get; } = new List<RootResource>();
        public List<Resource> Resources => Flatten(RootResources).ToList();

        public DataModel DataModel { get; }

        public ResourceCollection(DataModel dataModel)
        {
            DataModel = dataModel;
        }

        public RootResource Add(ClassModel c)
        {
            var resource = new RootResource();
            resource.Name = c.Name;
            resource.Class = c;
            resource.Class.MakePersisted();
            resource.Class.IsResource = true;
            resource.DataModel = DataModel;
            AddRootResource(resource);

            foreach (var property in c.Properties)
            {
                if (!property.IsCollection || property.BuiltInType != BuiltInType.Object)
                    continue;
                if (property.IsManyToMany)
                {
                    CreateManyToManyRelation(resource, property);
                } else
                {
                    CreateOneToManyRelation(resource, property);
                }
            }
            return resource;
        }


        private void AddRootResource(RootResource resource)
        {
            if (RootResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
            RootResources.Add(resource);
        }
        private void CreateOneToManyRelation(RootResource resource, Property property)
        {
            var rootResource = RootResources.FirstOrDefault(r => r.Class == property.Class);
            if (rootResource == null)
                rootResource = Add(property.Class);
            var nestedResource = resource.OneToMany(property.Class);
            nestedResource.IsReadonly = true;
            nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }
        private void CreateManyToManyRelation(RootResource resource, Property property)
        {
            var rootResource = RootResources.FirstOrDefault(r => r.Class == property.Class);
            if (rootResource == null)
                rootResource = Add(property.Class);
            var nestedResource = resource.ManyToMany(property.Class, $"{resource.Class.Name.Plural}{property.Class.Name.Plural}Pivot");
            nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
            nestedResource.Name.Plural = property.Name;
            nestedResource.RootResource = rootResource;
            rootResource.AlternateResources.Add(nestedResource);
        }

        private IEnumerable<Resource> Flatten(List<RootResource> resources)
        {
            foreach (var r in resources)
            {
                yield return r;
                foreach (var sr in r.NestedResources)
                    yield return sr;
            }
        }

        
        public void Load(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                Parse(property, out var _);
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
            ClassModel result = DataModel.Classes.FirstOrDefault(c => c.Name.Singular == className);
            bool newClass = result == null;
            if (result == null)
            {
                result = new ClassModel
                {
                    Name = className,
                    Properties = new List<Property>()
                };
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
            if (newClass) {
                DataModel.AddClass(result);
                Add(result);
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
    }
}