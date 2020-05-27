using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class DataSeedStore
    {
        private Dictionary<int, List<JObject>> DataSeed { get; set; } = new Dictionary<int, List<JObject>>();

        public event Action<Schema,JObject,Schema,JObject> onPush;

        public void Add<T>(Resource resource, IEnumerable<T> data) {
            DataSeed[resource.Schema.Id] = data.Select(i => JObject.FromObject(i)).ToList();
            PushSeedDataToProperties(resource.Schema);
            PushSeedDataToNestedResources(resource);
        }

        public List<JObject> Get(Schema schema)
        {
            DataSeed.TryGetValue(schema.Id, out var data);
            return data ?? new List<JObject>();
        }

        internal void PushSeedDataToProperties(Schema schema)
        {
            foreach (var property in schema.Properties)
            {
                if (property.Schema == null)
                    continue;
                foreach (var item in DataSeed[schema.Id])
                {
                    var token = item.GetValue(property.Name);
                    item.Remove(property.Name);
                    if (!DataSeed.TryGetValue(property.Schema.Id, out var dataList))
                    {
                        dataList = new List<JObject>();
                        DataSeed.Add(property.Schema.Id, dataList);
                    }
                    if (token is JObject obj)
                    {
                        dataList.Add(obj);
                        onPush?.Invoke(schema, item, property.Schema, obj);
                    }
                    else if (token is JArray array)
                    {
                        foreach (var o in array)
                        {
                            dataList.Add(o as JObject);
                            onPush?.Invoke(schema, item, property.Schema, o as JObject);
                        }
                    }
                    PushSeedDataToProperties(property.Schema);
                }
            }
        }

        
        internal void PushSeedDataToNestedResources(Resource resource)
        {
            foreach (var nestedResource in resource.NestedResources)
            {
                foreach (var item in DataSeed[resource.Schema.Id])
                {
                    var token = item.GetValue(nestedResource.Name);
                    item.Remove(nestedResource.Name);
                    if (!DataSeed.TryGetValue(nestedResource.Schema.Id, out var dataList))
                    {
                        dataList = new List<JObject>();
                        DataSeed.Add(nestedResource.Schema.Id, dataList);
                    }
                    if (token is JObject obj)
                    {
                        dataList.Add(obj);
                        onPush?.Invoke(resource.Schema, item, nestedResource.Schema, obj);
                    }
                    else if (token is JArray array)
                    {
                        foreach (var o in array)
                        {
                            dataList.Add(o as JObject);
                            onPush?.Invoke(resource.Schema, item, nestedResource.Schema, o as JObject);
                        }
                    }
                    PushSeedDataToProperties(nestedResource.Schema);
                    PushSeedDataToNestedResources(nestedResource);
                }
            }
        }
    }
}
