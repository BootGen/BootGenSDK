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
            return new OASSchema
            {
                Name = schema.Name,
                Properties = schema.Properties.Select(ConvertProperty<OASProperty>).ToList()
            };
        }

        private static T ConvertProperty<T>(Property property) where T : IOASProperty, new()
        {
            var oasProp = new T { Name = property.Name.ToLower() };
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
                case BuiltInType.Object:
                    oasProp.Reference = property.Schema.Name;
                    break;
            }
            oasProp.IsCollection = property.IsCollection;

            return oasProp;
        }

        private static List<Route> CreateRouteForResource(Resource resource, Path basePath)
        {
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.Name.ToLower();
            basePath = basePath.Adding(new PathComponent { Name = resourceName.ToLower() + (resource.IsCollection ? "s" : "") });
            route.Path = basePath.ToString();
            result.Add(route);
            route.Operations = new List<Operation>();
            if (resource.IsCollection)
            {
                AddCollectionOperations(resource, route, basePath);
                var subRoute = new Route();
                string itemIdName = resourceName.ToLower() + "Id";
                Parameter idParameter = ConvertProperty<Parameter>(resource.Schema.IdProperty);
                idParameter.Name = itemIdName;
                idParameter.Kind = "path";
                basePath = basePath.Adding(new PathComponent { Parameter = idParameter, Name = itemIdName });
                subRoute.Path = basePath.ToString();
                subRoute.Operations = new List<Operation>();
                result.Add(subRoute);
                AddItemOperations(resource, subRoute, basePath);
            }
            else
            {
                AddBaseOperations(resource, route, basePath);
            }
            foreach (var subResource in resource.Resoursces)
            {
                result.AddRange(CreateRouteForResource(subResource, basePath));
            }
            return result;
        }

        private static void AddCollectionOperations(Resource resource, Route route, Path path)
        {
            string resourceName = resource.Name.ToLower();
            if (resource.Get)
                route.Operations.Add(new Operation(Method.Get)
                {
                    Name = "get" + resource.Name + "s",
                    Summary = $"retrieve list of {resourceName}s",
                    Response = resource.Name,
                    ResponseIsCollection = true,
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Parameters = path.Parameters
                });
            if (resource.Put)
                route.Operations.Add(new Operation(Method.Put)
                {
                    Name = "update" + resource.Name + "s",
                    Summary = $"update list of {resourceName}s",
                    Body = resource.Name,
                    BodyIsCollection = true,
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Parameters = path.Parameters
                });
            if (resource.Patch)
                route.Operations.Add(new Operation(Method.Patch)
                {
                    Name = "update" + resource.Name + "ListElements",
                    Summary = $"update elements of {resourceName} list",
                    Body = resource.Name,
                    BodyIsCollection = true,
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Parameters = path.Parameters
                });
            if (resource.Delete)
                route.Operations.Add(new Operation(Method.Delete)
                {
                    Name = "delete" + resource.Name + "s",
                    Summary = $"delete all elements of {resourceName}s",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
        }

        private static void AddBaseOperations(Resource resource, Route route, Path path)
        {
            string resourceName = resource.Name.ToLower();
            if (resource.Get)
                route.Operations.Add(new Operation(Method.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    Response = resource.Name,
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Parameters = path.Parameters
                });
            if (resource.Put)
                route.Operations.Add(new Operation(Method.Put)
                {
                    Name = "update" + resource.Name,
                    Summary = $"update {resourceName} resource",
                    Body = resource.Name,
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Parameters = path.Parameters
                });
        }

        private static void AddItemOperations(Resource resource, Route subRoute, Path path)
        {
            string resourceName = resource.Name.ToLower();

            if (resource.Get)
                subRoute.Operations.Add(new Operation(Method.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Response = resource.Name,
                    Parameters = path.Parameters
                });
            if (resource.Put)
                subRoute.Operations.Add(new Operation(Method.Put)
                {
                    Name = "update" + resource.Name,
                    Summary = $"update {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Body = resource.Name,
                    Parameters = path.Parameters
                });
            if (resource.Delete)
                subRoute.Operations.Add(new Operation(Method.Delete)
                {
                    Name = "delete" + resource.Name,
                    Summary = $"delete {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
        }
    }
}