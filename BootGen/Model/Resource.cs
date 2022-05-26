using System.Collections.Generic;
using System.Linq;
using BootGen.Core;

namespace BootGen;

public abstract class Resource
{
    public Noun Name { get; set; }
    public Class Class { get; set; }
    public bool HasTimestamps  => Class.HasTimestamps;
    public bool IsReadonly { get; set; }
    internal DataModel DataModel { get; set; }

}

public class RootResource : Resource
{
    public List<NestedResource> NestedResources { get; } = new List<NestedResource>();
    public List<NestedResource> AlternateResources { get; } = new List<NestedResource>();

    public NestedResource OneToMany(Property property)
    {
        NestedResource resource = new NestedResource();
        resource.Name = property.Noun;
        resource.Class = property.Class;
        resource.DataModel = DataModel;
        resource.ParentRelation = new ParentRelation(this);
        NestedResources.Add(resource);
        return resource;
    }
    
    public NestedResource ManyToMany(Property property, string pivotName)
    {
        NestedResource resource = OneToMany(property);
        resource.Pivot = CreatePivot(this, resource, pivotName);
        return resource;
    }
    private Class CreatePivot(Resource parent, Resource resource, string name)
    {
        var pivotClass = DataModel.Classes.FirstOrDefault(c => c.Name == name);
        if (pivotClass != null)
            return pivotClass;
        pivotClass = new Class(name)
        {
            IsServerOnly = true,
            IsPivot = true
        };
        pivotClass.CreateId();
        pivotClass.AllProperties.Add(new Property {
                        Name = parent.Name.Plural + Class.IdName,
                        BuiltInType = BuiltInType.Int,
                        IsKey = true
                    });
        pivotClass.AllProperties.Add(new Property {
                        Name = parent.Name,
                        Noun = parent.Name,
                        BuiltInType = BuiltInType.Object,
                        Class = parent.Class
                    });
        pivotClass.AllProperties.Add(new Property {
                        Name = resource.Name.Plural + Class.IdName,
                        BuiltInType = BuiltInType.Int,
                        IsKey = true
                    });
        pivotClass.AllProperties.Add(new Property {
                        Name = resource.Name,
                        Noun = resource.Name,
                        BuiltInType = BuiltInType.Object,
                        Class = resource.Class
                    });
        pivotClass.Id = DataModel.Classes.Count;
        DataModel.Classes.Add(pivotClass);
        return pivotClass;
    }

}
public class NestedResource : Resource
{
    internal ParentRelation ParentRelation { get; set; }
    public Resource ParentResource => ParentRelation.Resource;
    public RootResource RootResource { get; set; }
    public Class Pivot { get; set; }
    public string ParentName =>  ParentRelation.Name;
}