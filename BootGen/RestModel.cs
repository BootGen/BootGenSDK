using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class RestModel
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Licence { get; set; }
        public string Url { get; set; }
        public List<Schema> Schemas { get; }
        public List<Route> Routes { get; }

        public RestModel(BootGenApi api)
        {
            Schemas = api.Schemas;
            Routes = new List<Route>();
            foreach (var resource in api.Resources)
            {
                Routes.AddRange(CreateRouteForResource(resource, new Path()));
            }
        }

        private static List<Route> CreateRouteForResource(Resource resource, Path basePath)
        {
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.Name.ToLower();
            basePath = basePath.Adding(new PathComponent { IsVariable = false, Name = resourceName.ToLower() });
            route.Path = basePath.ToString();
            result.Add(route);
            route.Operations = new List<Operation>();
            if (resource.Get)
                route.Operations.Add(new Operation(Method.Get)
                {
                    Name = "get" + resourceName,
                    Summary = $"retrieve {resourceName} resource",
                    Response = resource.Name
                });
            if (resource.Put)
                route.Operations.Add(new Operation(Method.Put)
                {
                    Name = "update" + resourceName,
                    Summary = $"update {resourceName} resource",
                    Body = resource.Name
                });
            if (resource.IsCollection)
            {
                if (resource.Patch)
                    route.Operations.Add(new Operation(Method.Patch)
                    {
                        Name = "update" + resourceName + "Elements",
                        Summary = $"update elements of {resourceName} resource"
                    });
                var subRoute = new Route();
                basePath = basePath.Adding(new PathComponent { IsVariable = true, Name = resourceName.ToLower() + "Id" });
                subRoute.Path = basePath.ToString();
                result.Add(subRoute);
                if (resource.Get)
                    subRoute.Operations.Add(new Operation(Method.Get)
                    {
                        Name = "get" + resourceName,
                        Summary = $"retrieve {resourceName} resource"
                    });
                if (resource.Put)
                    subRoute.Operations.Add(new Operation(Method.Put)
                    {
                        Name = "update" + resourceName,
                        Summary = $"update {resourceName} resource"
                    });
                if (resource.Delete)
                    subRoute.Operations.Add(new Operation(Method.Delete)
                    {
                        Name = "delete" + resourceName,
                        Summary = $"delete {resourceName} resource"
                    });
            }
            foreach (var subResource in resource.Resoursces)
            {
                result.AddRange(CreateRouteForResource(subResource, basePath));
            }
            return result;
        }
    }
}