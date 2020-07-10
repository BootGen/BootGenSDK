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
        private Dictionary<int, List<SeedData>> Data { get; set; } = new Dictionary<int, List<SeedData>>();

        protected virtual void OnDataSplit(SeedRecord parent, SeedRecord current, Property property, DataRelation relation)
        {

        }

        private SeedRecord ToSeedRecord(Schema schema, JObject obj)
        {
            var record = new SeedRecord { Name = schema.Name };
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
                    case JTokenType.Null:
                    case JTokenType.Undefined:
                        continue;
                    case JTokenType.Date:
                        var dateTime = (DateTime)property.Value;
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, $"new DateTime({dateTime.Year}, {dateTime.Month}, {dateTime.Day}, {dateTime.Hour}, {dateTime.Minute}, {dateTime.Second})"));
                        break;
                    case JTokenType.String:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, $"\"{property.Value.ToString()}\""));
                        break;
                    case JTokenType.Integer:
                        var pp = schema.Properties.First(p => p.Name == property.Name);
                        if (pp.BuiltInType == BuiltInType.Enum) {
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, $"{pp.EnumSchema.Name}.{pp.EnumSchema.Values[(int)property.Value]}"));
                        } else
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                    default:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                }
            }
            return record;
        }

        public void Add<T>(Resource resource, IEnumerable<T> data)
        {
            List<JObject> rawDataList = data.Select(i => JObject.FromObject(i)).ToList();
            Data[resource.Schema.Id] = rawDataList.Select(o => new SeedData(o, ToSeedRecord(resource.Schema, o))).ToList();
            PushSeedDataToProperties(resource.Schema);
            PushSeedDataToNestedResources(resource);
        }

        public List<SeedRecord> Get(Schema schema)
        {
            Data.TryGetValue(schema.Id, out var data);
            return data.Select(t => t.SeedRecord).ToList() ?? new List<SeedRecord>();
        }

        public List<SeedRecord> All()
        {
            return Data.SelectMany(i => i.Value).Select(t => t.SeedRecord).ToList();
        }

        internal void PushSeedDataToProperties(Schema schema)
        {
            foreach (var property in schema.Properties)
            {
                if (property.Schema == null)
                    continue;
                foreach (var item in Data[schema.Id])
                {
                    if (SplitData(item, property, property.Schema))
                        PushSeedDataToProperties(property.Schema);
                }
            }
        }

        private bool SplitData(SeedData item, Property property, Schema schema)
        {
            var token = item.JObject.GetValue(property.Name);
            item.JObject.Remove(property.Name);
            if (!Data.TryGetValue(schema.Id, out var dataList))
            {
                dataList = new List<SeedData>();
                Data.Add(schema.Id, dataList);
            }
            if (token is JObject obj)
            {
                SeedRecord record = ToSeedRecord(schema, obj);
                var id = record.GetId(schema);
                if (!dataList.Any(d => d.SeedRecord.GetId(schema) == id))
                {
                    dataList.Add(new SeedData(obj, record));
                }
                OnDataSplit(item.SeedRecord, record, property, DataRelation.ManyToOne);
                return true;
            }
            else if (token is JArray array)
            {
                foreach (var o in array)
                {
                    JObject jObj = o as JObject;
                    SeedRecord record = ToSeedRecord(schema, jObj);
                    var id = record.GetId(schema);
                    if (!dataList.Any(d => d.SeedRecord.GetId(schema) == id))
                    {
                        dataList.Add(new SeedData(jObj, record));
                    }
                    OnDataSplit(item.SeedRecord, record, property, DataRelation.OneToMany);
                }
                return true;
            }
            return false;
        }

        internal void PushSeedDataToNestedResources(Resource resource)
        {
            foreach (var nestedResource in resource.NestedResources)
            {
                foreach (var item in Data[resource.Schema.Id])
                {
                    var property = new Property {
                        Name = nestedResource.Name,
                        BuiltInType = BuiltInType.Object,
                        Schema = nestedResource.Schema
                    };
                    if (SplitData(item, property, nestedResource.Schema))
                    {
                        PushSeedDataToProperties(nestedResource.Schema);
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

        internal string GetId(Schema schema)
        {
            return Values.FirstOrDefault(kvp => kvp.Key == schema.IdProperty.Name).Value;
        }
    }

    public enum DataRelation
    {
        OneToMany,
        ManyToOne
    }
}
