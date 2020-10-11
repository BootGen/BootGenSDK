﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class ResourceCollection
    {
        public List<Resource> RootResources { get; } = new List<Resource>();
        public List<Resource> Resources => Flatten(RootResources).ToList();

        public DataModel DataModel { get; }

        public ResourceCollection(DataModel dataModel)
        {
            DataModel = dataModel;
        }
        
        public Resource Add<T>()
        {
            Resource resource = DataModel.ResourceBuilder.FromClass<T>();
            resource.DataModel = DataModel;
            if (RootResources.Any(r => r.Name == resource.Name))
                throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
            RootResources.Add(resource);
            return resource;
        }

        private IEnumerable<Resource> Flatten(List<Resource> resources)
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