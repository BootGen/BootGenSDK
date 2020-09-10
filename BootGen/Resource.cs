using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Resource
    {
        public string PluralName { get; set; }
        public string SingularName => PluralName.Substring(0, PluralName.Length - 1);
        public ClassModel Class { get; set; }
        public bool IsReadonly { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public List<Resource> ParentResources { get; set; }
        public Resource ParentResource => ParentResources.LastOrDefault();
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; internal set; }
        public bool HasTimestamps { get => Class.HasTimestamps; internal set => Class.HasTimestamps = value; }
        public bool Authenticate { get; internal set; }
        public bool GenerateControler { get; set; } = true;
        public bool GenerateServiceInterface { get; set; } = true;
        public bool GenerateService { get; set; } = true;
    }

}
