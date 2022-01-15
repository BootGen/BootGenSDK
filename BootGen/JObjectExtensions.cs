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
                JArray renamedArray = (property.Value as JArray).RenamingArrays(oldName, newName);
                if (property.Name == oldName)
                    result.Add(newName, renamedArray);
                else
                    result.Add(property.Name, renamedArray);
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
    public static JObject RenamingObjects(this JObject obj, string oldName, string newName)
    {
        var result = new JObject();
        foreach (var property in obj.Properties()) {
            if (property.Value.Type == JTokenType.Array)
            {
                result.Add(property.Name, (property.Value as JArray).RenamingObjects(oldName, newName));
            } else if (property.Value.Type == JTokenType.Object)
            {
                var renamedObject = (property.Value as JObject).RenamingObjects(oldName, newName);
                if (property.Name == oldName)
                    result.Add(newName, renamedObject);
                else
                    result.Add(property.Name, renamedObject);
            } else {
                result.Add(property.Name, property.Value);
            }
        }
        return result;
    }

    public static JArray RenamingObjects(this JArray array, string oldName, string newName)
    {
        var result = new JArray();
        foreach (var token in array) {
            if (token.Type == JTokenType.Object) {
                result.Add((token as JObject).RenamingObjects(oldName, newName));
            } else {
                result.Add(token);
            }
        }
        return result;
    }
}
