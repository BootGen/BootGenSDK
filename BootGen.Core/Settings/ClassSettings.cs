using System.Collections.Generic;

public class ClassSettings
{
    public Dictionary<string, PropertySettings> PropertySettings { get; set; }
    public bool HasTimestamps { get; set; }

    public bool LeftEquals(object obj)
    {
        var other = obj as ClassSettings;
        if (other == null)
            return false;
        if (other.HasTimestamps != HasTimestamps)
            return false;
        foreach (var p in PropertySettings) {
            if (!other.PropertySettings.TryGetValue(p.Key, out var settings))
                continue;
            if (!p.Value.Equals(settings))
                return false;
        }
        return true;
    }
}