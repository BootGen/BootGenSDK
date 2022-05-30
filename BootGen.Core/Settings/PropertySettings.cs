
namespace BootGen.Core;

public struct PropertySettings
{
    public string Name { get; set; }
    public string ClassName { get; set; }
    public string VisibleName { get; set; }
    public bool? IsReadOnly { get; set; }
    public bool IsHidden { get; set; }
    public bool? IsManyToMany { get; set; }
    public bool? ShowAsImage { get; set; }
}
