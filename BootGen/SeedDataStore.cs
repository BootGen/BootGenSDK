using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class SeedDataStore
    {
        private class SeedData {
            internal JObject JObject { get; set; }
            internal SeedRecord SeedRecord { get; set; }
            internal SeedData(JObject obj, SeedRecord record)
            {
                JObject = obj;
                SeedRecord = record;
            }
        }
        private Dictionary<int, List<SeedData>> Data { get; set; } = new Dictionary<int, List<SeedData>>();

        protected virtual void OnDataSplit(SeedRecord parent, SeedRecord current) {

        }

        private SeedRecord ToSeedRecord(string name, JObject obj)
        {
            var record = new SeedRecord { Name = name };
            foreach (var property in obj.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.None:
                    case JTokenType.Object:
                    case JTokenType.Array:
                    case JTokenType.Constructor:
                    case JTokenType.Property:
                    case JTokenType.Comment:
                        continue;
                    case JTokenType.String:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, $"\"{property.Value.ToString()}\""));
                        break;
                    default:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                }
            }
            return record;
        }

        public void Add<T>(Resource resource, IEnumerable<T> data) {
            List<JObject> rawDataList = data.Select(i => JObject.FromObject(i)).ToList();
            Data[resource.Schema.Id] = rawDataList.Select(o => new SeedData(o, ToSeedRecord(resource.Schema.Name, o))).ToList();
            PushSeedDataToProperties(resource.Schema);
            PushSeedDataToNestedResources(resource);
        }

        public List<SeedRecord> Get(Schema schema)
        {
            Data.TryGetValue(schema.Id, out var data);
            return data.Select(t => t.SeedRecord).ToList() ?? new List<SeedRecord>();
        }

        internal void PushSeedDataToProperties(Schema schema)
        {
            foreach (var property in schema.Properties)
            {
                if (property.Schema == null)
                    continue;
                foreach (var item in Data[schema.Id])
                {
                    SplitData(item, property.Name, property.Schema);
                    PushSeedDataToProperties(property.Schema);
                }
            }
        }

        private void SplitData(SeedData item, string propertyName, Schema schema)
        {
            var token = item.JObject.GetValue(propertyName);
            item.JObject.Remove(propertyName);
            if (!Data.TryGetValue(schema.Id, out var dataList))
            {
                dataList = new List<SeedData>();
                Data.Add(schema.Id, dataList);
            }
            if (token is JObject obj)
            {
                dataList.Add(new SeedData(obj, ToSeedRecord(schema.Name, obj)));
                OnDataSplit(item.SeedRecord, dataList.Last().SeedRecord);
            }
            else if (token is JArray array)
            {
                foreach (var o in array)
                {
                    JObject jObj = o as JObject;
                    dataList.Add(new SeedData(jObj, ToSeedRecord(schema.Name, jObj)));
                    OnDataSplit(item.SeedRecord, dataList.Last().SeedRecord);
                }
            }
        }

        internal void PushSeedDataToNestedResources(Resource resource)
        {
            foreach (var nestedResource in resource.NestedResources)
            {
                foreach (var item in Data[resource.Schema.Id])
                {
                    SplitData(item, nestedResource.Name, nestedResource.Schema);
                    PushSeedDataToProperties(nestedResource.Schema);
                    PushSeedDataToNestedResources(nestedResource);
                }
            }
        }
    }

    public class SeedRecord
    {
        public string Name { get; set; }
        public List<KeyValuePair<string, string>> Values { get; set; } = new List<KeyValuePair<string, string>>();
    }
}
