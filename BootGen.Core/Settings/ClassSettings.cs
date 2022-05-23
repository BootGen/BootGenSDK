using System.Collections.Generic;
using System.Linq;

namespace BootGen.Core;
public class ClassSettings
{
    public string Name { get; set; }
    public bool HasTimestamps { get; set; }
    public List<PropertySettings> PropertySettings { get; set; }

    public bool LeftEquals(object obj)
    {
        var other = obj as ClassSettings;
        if (other == null)
            return false;
        if (other.HasTimestamps != HasTimestamps)
            return false;
        var settingsDict = other.PropertySettings.ToDictionary(s => s.Name);
        foreach (var p in PropertySettings) {
            if (!settingsDict.TryGetValue(p.Name, out var settings))
                continue;
            if (!p.Equals(settings))
                return false;
        }
        return true;
    }
}