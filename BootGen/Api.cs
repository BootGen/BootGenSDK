using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Api
    {
        private ResourceCollection ResourceCollection { get; }
        public List<Resource> Resources => ResourceCollection.Resources;
        public List<RootResource> RootResources => ResourceCollection.RootResources;
        public List<NestedResource> NestedResources => ResourceCollection.RootResources.SelectMany(r => r.NestedResources).ToList();
        public DataModel DataModel => ResourceCollection.DataModel;
        public List<Route> Routes { get; } = new List<Route>();
        public string BaseUrl { get; set; }

        public Api(ResourceCollection resourceCollection)
        {
            ResourceCollection = resourceCollection;
            foreach (var resource in RootResources)
            {
                Routes.AddRange(resource.GetRoutes());
                foreach (var nestedResource in resource.NestedResources)
                {
                    DoNestingSanityChecks(nestedResource);
                    Routes.AddRange(nestedResource.GetRoutes());
                    if (nestedResource.Pivot != null)
                        DataModel.ClassCollection.Add(nestedResource.Pivot);
                }
            }
            
        }

        private static void DoNestingSanityChecks(NestedResource nestedResource)
        {
            if (nestedResource.Pivot != null && nestedResource.RootResource == null)
            {
                throw new Exception($"{nestedResource.Name.Plural} is declared as a Many-To-Many nested resource on {nestedResource.ParentResource.Name.Plural}, but is does not have an associated root resource.");
            }
            if (nestedResource.RootResource != null && nestedResource.RootResource.Class != nestedResource.Class)
            {
                throw new Exception($"Type mismatch: ${nestedResource.Name.Plural} has type ${nestedResource.Class.Name}, but its associated root resource has type {nestedResource.RootResource.Class.Name}");
            }
        }

    }
}