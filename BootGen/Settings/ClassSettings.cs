using System.Collections.Generic;

public class ClassSettings
{
    public string Name { get; set; }
    public string VisibleName { get; set; }
    public List<PropertySettings> PropertySettings { get; set; }
    public bool HasTimestamps { get; set; }
}