using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Resource
    {
        public string Name { get; set; }
        public string PluralName { get; set; }
        public ClassModel Class { get; set; }
        public Route Route { get; set; }
        public Route ItemRoute { get; set; }
        public Resource ParentResource => ParentRelation?.Resource;
        internal ParentRelation ParentRelation { get; set; }
        public List<Resource> NestedResources { get; set; }
        public ClassModel Pivot { get; set; }
        public bool HasTimestamps { get => Class.HasTimestamps; set => Class.HasTimestamps = value; }
        public bool Authenticate { get; set; }
        public bool IsReadonly { get; set; }
        public ResourceGenerationSettings GenerationSettings { get; } = new ResourceGenerationSettings();
    }

    public class ResourceGenerationSettings
    {        
        public bool GenerateController { get; set; } = true;
        public bool GenerateServiceInterface { get; set; } = true;
        public bool GenerateService { get; set; } = true;
        public string ControllerName { get; set; }
        public string ServiceName { get; set; }
    }

}
