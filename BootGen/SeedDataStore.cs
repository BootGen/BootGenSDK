using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class SeedDataStore
    {
        private const string PermissionTokenId = "PermissionTokenId";
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

        public List<PermissionToken> PermissionTokens { get; } = new List<PermissionToken>();
        public List<UserPermission> UserPermissions { get; } = new List<UserPermission>();

        public SeedDataStore(BootGenApi api)
        {
            this.classStore = api.ClassStore;
            this.resourceStore = api.ResourceStore;
        }

        private SeedRecord ToSeedRecord(ClassModel c, JObject obj)
        {
            var record = new SeedRecord { Name = c.Name };
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
                        var pp = c.Properties.First(p => p.Name == property.Name);
                        if (pp.BuiltInType == BuiltInType.Enum)
                        {
                            record.Values.Add(new KeyValuePair<string, string>(property.Name, $"{pp.Enum.Name}.{pp.Enum.Values[(int)property.Value]}"));
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
            List<SeedData> seedDataList = rawDataList.Select(o => new SeedData(o, ToSeedRecord(resource.Class, o))).ToList();
            Data[resource.Class.Id] = seedDataList;
            if (resource.Class.UsePermissions)
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
                                PermissionToken = token,
                                UserId = permission.Key,
                                Permission = permission.Value
                            });
                        }
                }

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
                    if (SplitData(item, property, property.Class))
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

        private bool SplitData(SeedData item, Property property, ClassModel c, ClassModel pivot = null)
        {
            var token = item.JObject.GetValue(property.Name);
            item.JObject.Remove(property.Name);
            var dataList = GetDataList(c);
            if (token is JObject obj)
            {
                SeedRecord record = ToSeedRecord(c, obj);
                var id = record.GetId(c);
                if (!dataList.Any(d => d.SeedRecord.GetId(c) == id))
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
                    SeedRecord record = ToSeedRecord(c, jObj);
                    var id = record.GetId(c);
                    if (!dataList.Any(d => d.SeedRecord.GetId(c) == id))
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
                        if (c.UsePermissions)
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
                    if (SplitData(item, property, nestedResource.Class, nestedResource.Pivot))
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

        internal string GetId(ClassModel c)
        {
            return Values.FirstOrDefault(kvp => kvp.Key == c.IdProperty.Name).Value;
        }
    }

    public class PermissionToken
    {
        public int Id { get; set; }
        public List<UserPermission> UserPermissions { get; set; }
    }

    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public PermissionToken PermissionToken { get; set; }
        public Permission Permission { get; set; }
    }

}
