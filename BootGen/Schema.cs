using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Schema
    {
        public string Name { get; internal set; }
        public Property IdProperty { get; internal set; }
        public List<Property> Properties { get; internal set; }
        public bool HasRequiredProperties => Properties.Any(p => p.IsRequired);
        public List<JObject> DataSeed { get; set; } = new List<JObject>();

        internal void InitDataSeed(IEnumerable data)
        {
            foreach (var item in data)
            {
                DataSeed.Add(JObject.FromObject(item));
            }
            PushSeedDataToProperties();
        }

        internal void PushSeedDataToProperties()
        {
            foreach (var property in Properties)
            {
                if (property.Schema == null)
                    continue;
                foreach (var item in DataSeed)
                {
                    var token = item.GetValue(property.Name);
                    item.Remove(property.Name);
                    if (token is JObject obj)
                    {
                        property.Schema.DataSeed.Add(obj);
                    }
                    else if (token is JArray array)
                    {
                        foreach (var o in array)
                        {
                            property.Schema.DataSeed.Add(o as JObject);
                        }
                    }
                    property.Schema.PushSeedDataToProperties();
                }
            }
        }
    }
}
