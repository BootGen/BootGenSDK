namespace BootGen.Core;

public class Property
{
    public Noun Noun { get; set; }
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            VisibleName = value.ToWords();
        }
    }
    public string VisibleName { get; set; }
    public bool IsReadOnly { get; set; }
    public BuiltInType BuiltInType { get; set; }
    public bool IsCollection { get; set; }
    public Class Class { get; set; }
    public bool IsServerOnly { get; set; }
    public Property MirrorProperty { get; set; }
    public bool IsParentReference { get; set; }
    public bool IsManyToMany { get; set; }
    public bool IsClientReadonly { get; set; }
    public bool IsKey { get; set; }
    public bool IsHidden { get; set; }
}


public enum BuiltInType { String, Int, Float, Bool, DateTime, Object }
