using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal class ResourceStore
    {
        public List<Resource> RootResources { get; } = new List<Resource>();
        public List<Resource> Resources => Flatten(RootResources).ToList();
        public void Add(Resource resource)
        {
            RootResources.Add(resource);
        }

        public IEnumerable<Resource> Flatten(List<Resource> resources)
        {
            foreach (var r in resources)
            {
                yield return r;
                foreach (var sr in Flatten(r.NestedResources))
                    yield return sr;
            }
        }
        public Resource GetRootResource(Resource resource)
        {
            return RootResources.FirstOrDefault(r => r.Class == resource.Class);
        }
    }
}