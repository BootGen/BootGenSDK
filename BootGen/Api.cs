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
                    Routes.AddRange(nestedResource.GetRoutes());
                }
            }
            
        }


    }
}