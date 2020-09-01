using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class SeedDataStore
    {
        private readonly ClassStore classStore;
        private readonly ResourceStore resourceStore;

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
        private Dictionary<int, List<SeedData>> Data { get; set; } = new Dictionary<int, List<SeedData>>();
        private Dictionary<int, int> NextClassIds = new Dictionary<int, int>();

        public SeedDataStore(BootGenApi api)
        {
            this.classStore = api.ClassStore;
            this.resourceStore = api.ResourceStore;
        }

        private int GetNextId(ClassModel c) {
            if (!NextClassIds.Keys.Contains(c.Id))
            {
                NextClassIds[c.Id] = 2;
                return 1;
            }
            int id = NextClassIds[c.Id];
            NextClassIds[c.Id] += 1;
            return id;
        }

        private SeedRecord ToSeedRecord(ClassModel c, JObject obj)
        {
            var record = new SeedRecord { Name = c.Name };
            foreach (var property in obj.Properties())
            {
                var classProperty = c.Properties.FirstOrDefault(p => p.Name == property.Name);
                if (classProperty != null && classProperty.Location == Location.ClientOnly)
                    continue;
                switch (property.Value.Type)
                {
                    case JTokenType.None:
                    case JTokenType.Object:
                    case JTokenType.Array:
                    case JTokenType.Constructor:
                    case JTokenType.Property:
                    case JTokenType.Comment:
                    case JTokenType.Null:
                    case JTokenType.Undefined:
                        continue;
                    case JTokenType.Date:
                        var dateTime = (DateTime)property.Value;
                        AddDateTime(record, property.Name, dateTime);
                        break;
                    case JTokenType.String:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, $"\"{property.Value.ToString()}\""));
                        break;
                    case JTokenType.Integer:
                        if (classProperty.BuiltInType == BuiltInType.Enum)
                        {
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, $"{classProperty.Enum.Name}.{classProperty.Enum.Values[(int)property.Value]}"));
                        }
                        else
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                    default:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                }
            }
            if (c.HasTimestamps) {
                AddDateTime(record, "Created", DateTime.Now);
                AddDateTime(record, "Updated", DateTime.Now);
            }
            if (c.Persisted) {
                record.Values.Add(new KeyValuePair<string, string>("Id", GetNextId(c).ToString()));
            }
            if (c.IsResource) {
                record.Values.Add(new KeyValuePair<string, string>("Uuid", Guid.NewGuid().ToString()));
            }
            return record;
        }

        private static void AddDateTime(SeedRecord record, string propertyName, DateTime dateTime)
        {
            record.Values.Add(new KeyValuePair<string, string>(propertyName, $"new DateTime({dateTime.Year}, {dateTime.Month}, {dateTime.Day}, {dateTime.Hour}, {dateTime.Minute}, {dateTime.Second})"));
        }

        public void Add<T>(Resource resource, IEnumerable<T> data)
        {
            List<JObject> rawDataList = data.Select(i => JObject.FromObject(i)).ToList();
            List<SeedData> seedDataList = rawDataList.Select(o => new SeedData(o, ToSeedRecord(resource.Class, o))).ToList();
            Data[resource.Class.Id] = seedDataList;

            PushSeedDataToProperties(resource.Class);
            PushSeedDataToNestedResources(resource);
        }

        public List<SeedRecord> Get(ClassModel c)
        {
            Data.TryGetValue(c.Id, out var data);
            return data.Select(t => t.SeedRecord).ToList() ?? new List<SeedRecord>();
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
                foreach (var item in Data[c.Id])
                {
                    if (SplitData(item, property))
                    {
                        PushSeedDataToProperties(property.Class);
                        foreach (var resource in resourceStore.Resources)
                        {
                            if (resource.Class == property.Class)
                                PushSeedDataToNestedResources(resource);
                        }
                    }
                }
            }
        }

        private bool SplitData(SeedData item, Property property, ClassModel pivot = null)
        {
            var token = item.JObject.GetValue(property.Name);
            item.JObject.Remove(property.Name);
            var dataList = GetDataList(property.Class);
            if (token is JObject obj)
            {
                SeedRecord record = ToSeedRecord(property.Class, obj);
                var id = record.GetId();
                if (!dataList.Any(d => d.SeedRecord.GetId() == id))
                {
                    dataList.Add(new SeedData(obj, record));
                }
                item.SeedRecord.Values.Add(new KeyValuePair<string, string>(property.Name + "Id", record.Values.First(kvp => kvp.Key.ToLower() == "id").Value));
                return true;
            }
            else if (token is JArray array)
            {
                foreach (var o in array)
                {
                    JObject jObj = o as JObject;
                    SeedRecord record = ToSeedRecord(property.Class, jObj);
                    var id = record.GetId();
                    if (!dataList.Any(d => d.SeedRecord.GetId() == id))
                    {
                        dataList.Add(new SeedData(jObj, record));
                    }
                    if (pivot != null)
                    {
                        var pivotDataList = GetDataList(pivot);
                        var pivotRecord = new SeedRecord
                        {
                            Name = pivot.Name,
                            Values = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Id", (pivotDataList.Count + 1).ToString()) }
                        };
                        pivotRecord.Values.Add(new KeyValuePair<string, string>(item.SeedRecord.Name + "Id", item.SeedRecord.GetId()));
                        pivotRecord.Values.Add(new KeyValuePair<string, string>(record.Name + "Id", record.GetId()));
                        pivotDataList.Add(new SeedData(null, pivotRecord));
                    }
                    else
                    {
                        record.Values.Add(new KeyValuePair<string, string>(item.SeedRecord.Name + "Id", item.SeedRecord.GetId()));
                        string parentUuid = item.SeedRecord.GetUuid();
                        if (!string.IsNullOrEmpty(parentUuid))
                            record.Values.Add(new KeyValuePair<string, string>(item.SeedRecord.Name + "Uuid", parentUuid));
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

        internal void PushSeedDataToNestedResources(Resource resource)
        {
            foreach (var nestedResource in resource.NestedResources)
            {
                foreach (var item in Data[resource.Class.Id])
                {
                    var property = new Property
                    {
                        Name = nestedResource.PluralName,
                        BuiltInType = BuiltInType.Object,
                        Class = nestedResource.Class
                    };
                    if (SplitData(item, property, nestedResource.Pivot))
                    {
                        PushSeedDataToProperties(nestedResource.Class);
                        PushSeedDataToNestedResources(nestedResource);
                    }
                }
            }
        }
    }

    public class SeedRecord
    {
        public string Name { get; set; }
        public List<KeyValuePair<string, string>> Values { get; set; } = new List<KeyValuePair<string, string>>();

        internal string GetId()
        {
            return Values.FirstOrDefault(kvp => kvp.Key == "Id").Value;
        }
        internal string GetUuid()
        {
            return Values.FirstOrDefault(kvp => kvp.Key == "Uuid").Value;
        }
    }

}
