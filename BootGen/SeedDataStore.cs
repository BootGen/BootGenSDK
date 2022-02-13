using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class SeedDataStore
    {

        private class SeedData
        {
            internal JObject JObject { get; set; }
            internal SeedRecord SeedRecord { get; set; }
            internal SeedData(JObject obj, SeedRecord record)
            {
                JObject = obj;
                SeedRecord = record;
            }
        }
        private Dictionary<int, List<SeedData>> Data { get; } = new Dictionary<int, List<SeedData>>();
        private Dictionary<int, int> NextClassIds = new Dictionary<int, int>();
        private readonly ResourceCollection resourceCollection;

        public SeedDataStore(ResourceCollection resourceCollection)
        {
            this.resourceCollection = resourceCollection;
        }

        private string GetNextId(ClassModel c)
        {
            if (!NextClassIds.Keys.Contains(c.Id))
            {
                NextClassIds[c.Id] = 2;
                return 1.ToString();
            }
            int id = NextClassIds[c.Id];
            NextClassIds[c.Id] += 1;
            return id.ToString();
        }

        private SeedRecord ToSeedRecord(ClassModel c, JObject obj)
        {
            var record = new SeedRecord { Name = c.Name };
            foreach (var property in obj.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Date:
                        var dateTime = (DateTime)property.Value;
                        record.Set(property.Name, $"new DateTime({dateTime.Year}, {dateTime.Month}, {dateTime.Day}, {dateTime.Hour}, {dateTime.Minute}, {dateTime.Second})");
                        break;
                    case JTokenType.String:
                        record.Set(property.Name, $"\"{property.Value.ToString()}\"");
                        break;
                    case JTokenType.Integer:
                        record.Set(property.Name, property.Value.ToString());
                        break;
                    case JTokenType.Float:
                        record.Set(property.Name, $"{property.Value.ToString()}f");
                        break;
                    case JTokenType.Boolean:
                        record.Set(property.Name, ((bool)property.Value).ToString().ToLower());
                        break;
                }
            }
            if (c.HasTimestamps)
            {
                record.Set("Created", "DateTime.Now");
                record.Set("Updated", "DateTime.Now");
            }
            record.Values.Insert(0, KeyValuePair.Create("Id", GetNextId(c)));
            return record;
        }

        public void Add(RootResource resource, List<JObject> rawDataList)
        {
            List<SeedData> seedDataList = rawDataList.Select(o => new SeedData(o, ToSeedRecord(resource.Class, o))).ToList();
            Data[resource.Class.Id] = seedDataList;

            PushSeedDataToNestedResources(resource);
            PushSeedDataToProperties(resource.Class);
        }

        public List<SeedRecord> Get(ClassModel c)
        {
            Data.TryGetValue(c.Id, out var data);
            return data.Select(t => t.SeedRecord).ToList();
        }

        public List<SeedRecord> All()
        {
            return Data.SelectMany(i => i.Value).Select(t => t.SeedRecord).ToList();
        }

        internal void PushSeedDataToProperties(ClassModel c)
        {
            foreach (var property in c.Properties)
            {
                if (property.Class == null)
                    continue;
                var seedDataList = new List<SeedData>(Data[c.Id]);
                foreach (var item in seedDataList)
                {
                    if (SplitData(item, property))
                    {
                        PushSeedDataToProperties(property.Class);
                        foreach (var resource in resourceCollection.RootResources)
                        {
                            if (resource.Class == property.Class)
                                PushSeedDataToNestedResources(resource);
                        }
                    }
                }
            }
        }

        private bool SplitData(SeedData item, Property property)
        {
            var token = item.JObject.GetValue(property.Name);
            item.JObject.Remove(property.Name);
            var dataList = GetDataList(property.Class);
            if (token is JObject obj)
            {
                SeedRecord record = ToSeedRecord(property.Class, obj);
                dataList.Add(new SeedData(obj, record));
                item.SeedRecord.Values.Add(new KeyValuePair<string, string>(property.Name + "Id", record.GetId()));
                return true;
            }
            return false;
        }

        private bool SplitData(SeedData item, NestedResource nestedResource)
        {
            var token = item.JObject.GetValue(nestedResource.Name.Plural);
            item.JObject.Remove(nestedResource.Name.Plural);
            var dataList = GetDataList(nestedResource.Class);
            if (token is JArray array)
            {
                foreach (var o in array)
                {
                    JObject jObj = o as JObject;
                    if (jObj == null)
                        continue;
                    SeedRecord record = ToSeedRecord(nestedResource.Class, jObj);
                    var id = record.GetId();
                    if (!dataList.Any(d => d.SeedRecord.GetId() == id))
                    {
                        dataList.Add(new SeedData(jObj, record));
                    }
                    if (nestedResource.Pivot != null)
                    {
                        var pivotDataList = GetDataList(nestedResource.Pivot);
                        var pivotRecord = new SeedRecord
                        {
                            Name = nestedResource.Pivot.Name,
                            IsPivot = true,
                            Values = new List<KeyValuePair<string, string>> { 
                                new KeyValuePair<string, string>(item.SeedRecord.Name.Plural + "Id", item.SeedRecord.GetId()),
                                new KeyValuePair<string, string>(nestedResource.Name.Plural + "Id", record.GetId())
                             }
                        };
                        pivotDataList.Add(new SeedData(null, pivotRecord));
                    }
                    else
                    {
                        record.Values.Add(new KeyValuePair<string, string>(nestedResource.ParentRelation.Name + "Id", item.SeedRecord.GetId()));
                    }

                }
                return true;
            }
            return false;
        }

        private List<SeedData> GetDataList(ClassModel c)
        {
            if (!Data.TryGetValue(c.Id, out var dataList))
            {
                dataList = new List<SeedData>();
                Data.Add(c.Id, dataList);
            }

            return dataList;
        }

        internal void PushSeedDataToNestedResources(RootResource resource)
        {
            foreach (var nestedResource in resource.NestedResources)
            {
                foreach (var item in Data[resource.Class.Id].ToList())
                {
                    if (SplitData(item, nestedResource))
                    {
                        if (nestedResource.RootResource != null)
                            PushSeedDataToNestedResources(nestedResource.RootResource);
                        PushSeedDataToProperties(nestedResource.Class);
                    }
                }
            }
        }

        public void Load(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                if (property.Value.Type == JTokenType.Array) {
                    var resource = resourceCollection.RootResources.First(r => r.Name.Plural.ToLower() == property.Name.ToLower());
                    var data = (property.Value as JArray).Select(t => t as JObject).Where(t => t != null).ToList();
                    foreach (var item in data)
                        item.Capitalize();
                    Add(resource, data);
                } else {
                    var resource = resourceCollection.RootResources.First(r => r.Name.Singular.ToLower() == property.Name.ToLower());
                    var data = property.Value as JObject;
                    data.Capitalize();
                    Add(resource, new List<JObject>{ data });
                }
            }
        }
    }

    public class SeedRecord
    {
        public Noun Name { get; set; }
        public bool IsPivot { get; set; }
        public List<KeyValuePair<string, string>> Values { get; set; } = new List<KeyValuePair<string, string>>();

        internal string GetId()
        {
            return Values.FirstOrDefault(kvp => kvp.Key == "Id").Value;
        }

        public string Get(string key)
        {
            return Values.FirstOrDefault(kvp => kvp.Key == key).Value;
        }

        internal void Set(string key, string value)
        {
            Values.Add(new KeyValuePair<string, string>(key, value));
        }
    }

}
