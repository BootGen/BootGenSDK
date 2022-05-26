using System;
using System.Collections.Generic;
using System.Linq;
using BootGen.Core;

namespace BootGen;

public class ResourceCollection
{
    public List<RootResource> RootResources { get; } = new List<RootResource>();
    public List<NestedResource> NestedResources => RootResources.SelectMany(r => r.NestedResources).ToList();

    public DataModel DataModel { get; }

    public ResourceCollection(DataModel dataModel)
    {
        DataModel = dataModel;

        var classes = new List<Class>(DataModel.Classes);
        foreach (var c in classes)
            AddRootResource(c);
        foreach (var c in classes)
            AddNestedResources(c);
    }

    public void AddRootResource(Class @class)
    {
        var resource = new RootResource();
        resource.Name = @class.Name;
        resource.Class = @class;
        resource.DataModel = DataModel;
        RootResources.Add(resource);
    }
    public void AddNestedResources(Class @class)
    {
        var resource = RootResources.First(r => r.Class == @class);
        foreach (var property in @class.Properties)
        {
            if (!property.IsCollection || property.BuiltInType != BuiltInType.Object)
                continue;
            if (property.IsManyToMany)
            {
                CreateManyToManyRelation(resource, property);
            }
            else
            {
                CreateOneToManyRelation(resource, property);
            }
        }
    }


    private void CreateOneToManyRelation(RootResource resource, Property property)
    {
        var rootResource = RootResources.First(r => r.Class == property.Class);
        var nestedResource = resource.OneToMany(property);
        nestedResource.IsReadonly = true;
        nestedResource.RootResource = rootResource;
        rootResource.AlternateResources.Add(nestedResource);
    }
    private void CreateManyToManyRelation(RootResource resource, Property property)
    {
        var rootResource = RootResources.First(r => r.Class == property.Class);
        string pivotName;
        if (resource.Class == property.Class || string.Compare(resource.Class.Name, property.Noun, StringComparison.InvariantCulture) < 0)
            pivotName = $"{resource.Class.Name}{property.Noun}";
        else
            pivotName = $"{property.Noun}{resource.Class.Name}";
        var nestedResource = resource.ManyToMany(property, pivotName);
        nestedResource.Name = property.Name.Substring(0, property.Name.Length - 1);
        nestedResource.Name.Plural = property.Name;
        nestedResource.RootResource = rootResource;
        rootResource.AlternateResources.Add(nestedResource);
    }

}