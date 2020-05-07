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
        public List<OASSchema> Schemas { get; }
        public List<Route> Routes { get; }

        public RestModel(BootGenApi api)
        {
            Schemas = api.Schemas.Select(ConvertSchema).ToList();
            Routes = new List<Route>();
            foreach (var resource in api.Resources)
            {
                Routes.AddRange(CreateRouteForResource(resource, new Path()));
            }
        }

        private OASSchema ConvertSchema(Schema schema)
        {
            var result = new OASSchema {
                Name = schema.Name,
                Properties = new List<OASProperty>()
            };

            foreach (var property in schema.Properties)
            {
                var oasProp = new OASProperty{ Name = property.Name.ToLower() };
                switch (property.Type)
                {
                    case BuiltInType.Bool:
                    oasProp.Required = true;
                    oasProp.Type = "boolean";
                    break;
                    case BuiltInType.Int32:
                    oasProp.Required = true;
                    oasProp.Type = "integer";
                    oasProp.Format = "int32";
                    break;
                    case BuiltInType.Int64:
                    oasProp.Required = true;
                    oasProp.Type = "integer";
                    oasProp.Format = "int64";
                    break;
                    case BuiltInType.String:
                    oasProp.Required = false;
                    oasProp.Type = "string";
                    break;
                }
                result.Properties.Add(oasProp);
            }

            return result;
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
            AddBaseOperations(resource, route);
            if (resource.IsCollection)
            {
                AddCollectionOperations(resource, route);
                var subRoute = new Route();
                basePath = basePath.Adding(new PathComponent { IsVariable = true, Name = resourceName.ToLower() + "Id" });
                subRoute.Path = basePath.ToString();
                result.Add(subRoute);
                AddItemOperations(resource, subRoute);
            }
            foreach (var subResource in resource.Resoursces)
            {
                result.AddRange(CreateRouteForResource(subResource, basePath));
            }
            return result;
        }

        private static void AddCollectionOperations(Resource resource, Route route)
        {
            string resourceName = resource.Name.ToLower();
            if (resource.Patch)
                route.Operations.Add(new Operation(Method.Patch)
                {
                    Name = "update" + resource.Name + "elements",
                    Summary = $"update elements of {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update"
                });
            if (resource.Delete)
                route.Operations.Add(new Operation(Method.Patch)
                {
                    Name = "delete all" + resource.Name + "elements",
                    Summary = $"delete all elements of {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion"
                });
        }

        private static void AddBaseOperations(Resource resource, Route route)
        {
            string resourceName = resource.Name.ToLower();
            if (resource.Get)
                route.Operations.Add(new Operation(Method.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    Response = resource.Name,
                    SuccessCode = 200,
                    SuccessDescription = $"successful query"
                });
            if (resource.Put)
                route.Operations.Add(new Operation(Method.Put)
                {
                    Name = "update" + resource.Name,
                    Summary = $"update {resourceName} resource",
                    Body = resource.Name,
                    SuccessCode = 200,
                    SuccessDescription = $"successful update"
                });
        }

        private static void AddItemOperations(Resource resource, Route subRoute)
        {
            string resourceName = resource.Name.ToLower();
            if (resource.Get)
                subRoute.Operations.Add(new Operation(Method.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query"
                });
            if (resource.Put)
                subRoute.Operations.Add(new Operation(Method.Put)
                {
                    Name = "update" + resource.Name,
                    Summary = $"update {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update"
                });
            if (resource.Delete)
                subRoute.Operations.Add(new Operation(Method.Delete)
                {
                    Name = "delete" + resource.Name,
                    Summary = $"delete {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion"
                });
        }
    }

    public class OASSchema 
    {
        public string Name { get; set; }
        public List<OASProperty> Properties { get; set; }
        public bool HasRequiredProperties => Properties.Any(p => p.Required);
    }
    public class OASProperty
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
    }
}