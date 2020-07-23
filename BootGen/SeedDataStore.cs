using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class SeedDataStore
    {
        private const string PermissionTokenId = "PermissionTokenId";
        private readonly SchemaStore schemaStore;
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

        public List<PermissionToken> PermissionTokens { get; } = new List<PermissionToken>();
        public List<UserPermission> UserPermissions { get; } = new List<UserPermission>();

        public SeedDataStore(SchemaStore schemaStore, ResourceStore resourceStore)
        {
            this.schemaStore = schemaStore;
            this.resourceStore = resourceStore;
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
                        if (pp.BuiltInType == BuiltInType.Enum)
                        {
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, $"{pp.EnumSchema.Name}.{pp.EnumSchema.Values[(int)property.Value]}"));
                        }
                        else
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                    default:
                        record.Values.Add(new KeyValuePair<string, string>(property.Name, property.Value.ToString()));
                        break;
                }
            }
            return record;
        }

        public void Add<T>(Resource resource, IEnumerable<T> data, Dictionary<int, Permission> permissions = null)
        {
            List<JObject> rawDataList = data.Select(i => JObject.FromObject(i)).ToList();
            List<SeedData> seedDataList = rawDataList.Select(o => new SeedData(o, ToSeedRecord(resource.Schema, o))).ToList();
            Data[resource.Schema.Id] = seedDataList;
            if (resource.Schema.UsePermissions)
                foreach (var seedData in seedDataList)
                {
                    if (seedData.SeedRecord.Values.Any(t => t.Key == PermissionTokenId))
                        continue;
                    var token = new PermissionToken { Id = PermissionTokens.Count + 1 };
                    PermissionTokens.Add(token);
                    seedData.SeedRecord.Values.Add(new KeyValuePair<string, string>(PermissionTokenId, token.Id.ToString()));
                    if (permissions != null)
                        foreach (var permission in permissions)
                        {
                            UserPermissions.Add(new UserPermission
                            {
                                Id = UserPermissions.Count + 1,
                                PermissionTokenId = token.Id,
                                UserId = permission.Key,
                                Permission = permission.Value
                            });
                        }
                }

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
                    {
                        PushSeedDataToProperties(property.Schema);
                        foreach (var resource in resourceStore.Resources)
                        {
                            if (resource.Schema == property.Schema)
                                PushSeedDataToNestedResources(resource);
                        }
                    }
                }
            }
        }

        private bool SplitData(SeedData item, Property property, Schema schema, Schema pivot = null)
        {
            var token = item.JObject.GetValue(property.Name);
            item.JObject.Remove(property.Name);
            var dataList = GetDataList(schema);
            if (token is JObject obj)
            {
                SeedRecord record = ToSeedRecord(schema, obj);
                var id = record.GetId(schema);
                if (!dataList.Any(d => d.SeedRecord.GetId(schema) == id))
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
                    SeedRecord record = ToSeedRecord(schema, jObj);
                    var id = record.GetId(schema);
                    if (!dataList.Any(d => d.SeedRecord.GetId(schema) == id))
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
                        pivotRecord.Values.Add(new KeyValuePair<string, string>(item.SeedRecord.Name + "Id", item.SeedRecord.Values.First(kvp => kvp.Key.ToLower() == "id").Value));
                        pivotRecord.Values.Add(new KeyValuePair<string, string>(record.Name + "Id", record.Values.First(kvp => kvp.Key.ToLower() == "id").Value));
                        pivotDataList.Add(new SeedData(null, pivotRecord));
                    }
                    else
                    {
                        record.Values.Add(new KeyValuePair<string, string>(item.SeedRecord.Name + "Id", item.SeedRecord.Values.First(kvp => kvp.Key.ToLower() == "id").Value));
                        if (schema.UsePermissions)
                        {
                            var tokenId = item.SeedRecord.Values.FirstOrDefault(t => t.Key == PermissionTokenId);
                            if (!string.IsNullOrEmpty(tokenId.Value))
                            {
                                record.Values.Add(new KeyValuePair<string, string>(PermissionTokenId, tokenId.Value));
                            }
                        }
                    }

                }
                return true;
            }
            return false;
        }

        private List<SeedData> GetDataList(Schema schema)
        {
            if (!Data.TryGetValue(schema.Id, out var dataList))
            {
                dataList = new List<SeedData>();
                Data.Add(schema.Id, dataList);
            }

            return dataList;
        }

        internal void PushSeedDataToNestedResources(Resource resource)
        {
            foreach (var nestedResource in resource.NestedResources)
            {
                foreach (var item in Data[resource.Schema.Id])
                {
                    var property = new Property
                    {
                        Name = nestedResource.PluralName,
                        BuiltInType = BuiltInType.Object,
                        Schema = nestedResource.Schema
                    };
                    if (SplitData(item, property, nestedResource.Schema, nestedResource.Pivot))
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

    public class PermissionToken
    {
        public int Id { get; set; }
    }

    public class UserPermission
    {
        public int Id { get; set; }
        public int PermissionTokenId { get; set; }
        public int UserId { get; set; }
        public Permission Permission { get; set; }
    }

}
