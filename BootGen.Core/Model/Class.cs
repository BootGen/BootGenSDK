using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BootGen.Core;

public class Class
{
    static public string IdName { get; } = "Id";
    public int Id { get; set; }
    public Noun Name { get; set; }
    public bool IsServerOnly { get; set; }
    public bool HasTimestamps { get; set; }
    public bool IsPivot { get; set; }
    public bool IsRoot { get; set; }
    public bool ReferredSingle { get; set; }
    public bool ReferredPlural { get; set; }
    public Property IdProperty => PropertyWithName(IdName);
    public List<Property> AllProperties { get; }
    public List<Property> Properties => AllProperties.Where(p => !p.IsHidden).ToList();
    public List<Property> CommonProperties => Properties.Where(p => !p.IsServerOnly).ToList();
    public List<Property> PrimitiveProperties => Properties.Where(p => p.BuiltInType != BuiltInType.Object && !p.IsKey).ToList();
    public List<Property> JsonProperties => Properties.Where(p => !p.IsKey && !p.IsParentReference).ToList();
    public List<Property> SettingsProperties => AllProperties.Where(p => !p.IsKey && !p.IsParentReference).ToList();
    public List<Property> ChildReferences => Properties.Where(p => p.BuiltInType == BuiltInType.Object && !p.IsParentReference).ToList();
    public bool HasChild => ChildReferences.Any();

    public bool IsEmpty => AllProperties.All(p => p.Name == IdName);

    public Class(string name)
    {
        Name = name;
        AllProperties = new List<Property>();
    }

    public void CreateId()
    {
        AllProperties.Insert(0, new Property
            {
                Name = Class.IdName,
                BuiltInType = BuiltInType.Int,
                IsClientReadonly = true,
                IsKey = true
            });
    }

    public Property PropertyWithName(string name)
    {
        return Properties.FirstOrDefault(p => p.Name == name);
    }
}