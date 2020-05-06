using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class BootGenApi
    {
        private readonly SchemaStore schemaStore;
        private readonly ResourceBuilder resourceBuilder;
        public List<Resource> Resources { get; } = new List<Resource>();
        public List<Schema> Schemas => schemaStore.Schemas;

        public BootGenApi() {
            schemaStore = new SchemaStore();
            resourceBuilder = new ResourceBuilder(schemaStore);
        }
        public Resource AddResource<T>()
        {
            Resource resource = resourceBuilder.FromClass<T>();
            Resources.Add(resource);
            return resource;
        }
    }
}