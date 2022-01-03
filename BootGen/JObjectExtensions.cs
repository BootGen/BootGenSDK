using Newtonsoft.Json.Linq;

namespace BootGen;
public static class JObjectExtensions
{
    public static JObject RenamingArrays(this JObject obj, string oldName, string newName)
    {
        var result = new JObject();
        foreach (var property in obj.Properties()) {
            if (property.Value.Type == JTokenType.Array)
            {
                if (property.Name == oldName)
                    result.Add(newName, (property.Value as JArray).RenamingArrays(oldName, newName));
                else
                    result.Add(property.Name, (property.Value as JArray).RenamingArrays(oldName, newName));
            } else if (property.Value.Type == JTokenType.Object)
            {
                result.Add(property.Name, (property.Value as JObject).RenamingArrays(oldName, newName));
            } else {
                result.Add(property.Name, property.Value);
            }
        }
        return result;
    }

    public static JArray RenamingArrays(this JArray array, string oldName, string newName)
    {
        var result = new JArray();
        foreach (var token in array) {
            if (token.Type == JTokenType.Object) {
                result.Add((token as JObject).RenamingArrays(oldName, newName));
            } else {
                result.Add(token);
            }
        }
        return result;
    }
}
