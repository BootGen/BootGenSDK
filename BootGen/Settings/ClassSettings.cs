using System.Collections.Generic;

public class ClassSettings
{
    public string VisibleName { get; set; }
    public Dictionary<string, PropertySettings> PropertySettings { get; set; }
    public bool HasTimestamps { get; set; }
}