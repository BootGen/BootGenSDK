using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class RestResource : Resource
    {
        public Route Route { get; set; }

        public new List<RestResource> Resources => base.Resources.Select(r => (RestResource)r).ToList();

        public RestResource(Resource resource)
        {
            IsCollection = resource.IsCollection;
            Schema = resource.Schema;
            Get = resource.Get;
            Put = resource.Put;
            Patch = resource.Patch;
            Delete = resource.Delete;
            base.Resources = resource.Resources.Select(r => new RestResource(r) as Resource).ToList();
        }
    }
}
