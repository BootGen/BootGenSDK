using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public static class RouteBuilder
    {
        public static List<Route> GetRoutes(this Resource resource, Path basePath)
        {
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.Name.ToCamelCase();
            basePath = basePath.Adding(new PathComponent { Name = resourceName + (resource.IsCollection ? "s" : "") });
            route.PathModel = basePath;
            result.Add(route);
            resource.Route = route; 
            route.Operations = new List<Operation>();
            if (resource.IsCollection)
            {
                AddCollectionOperations(resource, route, basePath);
                var subRoute = new Route();
                string itemIdName = resourceName + "Id";
                Parameter idParameter = resource.Schema.IdProperty.ConvertToParameter();
                idParameter.Name = itemIdName;
                idParameter.Kind = RestParamterKind.Path;
                basePath = basePath.Adding(new PathComponent { Parameter = idParameter, Name = itemIdName });
                subRoute.PathModel = basePath;
                subRoute.Operations = new List<Operation>();
                result.Add(subRoute);
                resource.ElementRoute = subRoute;
                AddItemOperations(resource, subRoute, basePath);
            }
            else
            {
                AddBaseOperations(resource, route, basePath);
            }
            foreach (var subResource in resource.NestedResources)
            {
                result.AddRange(subResource.GetRoutes(basePath));
            }
            return result;
        }

        public static IEnumerable<Route> GetRoutes(this Controller controller)
        {

                var path = new Path { new PathComponent { Name = controller.Name.ToKebabCase() } };
                foreach (var method in controller.Methods)
                {
                    yield return new Route
                    {
                        PathModel = path.Adding(new PathComponent { Name = method.Name.ToKebabCase() }),
                        Operations = new List<Operation> {
                            new Operation(HttpMethod.Post)
                            {
                                Name = method.Name.ToCamelCase(),
                                Parameters = method.Parameters.Select(ToQueryParam).ToList(),
                                Response = method.ReturnType.Schema?.Name,
                                ResponseIsCollection = method.ReturnType.IsCollection,
                                SuccessCode = 200,
                                SuccessDescription = method.Name + " success"
                            }
                        }
                    };
                }
        }

        private static Parameter ToQueryParam(Property p)
        {
            Parameter parameter = p.ConvertToParameter();
            parameter.Kind = RestParamterKind.Query;
            return parameter;
        }
        private static void AddCollectionOperations(Resource resource, Route route, Path path)
        {
            string resourceName = resource.Name.ToWords();
            if (resource.Get)
                route.Operations.Add(new Operation(HttpMethod.Get)
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
                route.Operations.Add(new Operation(HttpMethod.Put)
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
                route.Operations.Add(new Operation(HttpMethod.Patch)
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
                route.Operations.Add(new Operation(HttpMethod.Delete)
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
            string resourceName = resource.Name.ToWords();
            if (resource.Get)
                route.Operations.Add(new Operation(HttpMethod.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    Response = resource.Name,
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Parameters = path.Parameters
                });
            if (resource.Put)
                route.Operations.Add(new Operation(HttpMethod.Put)
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
            string resourceName = resource.Name.ToWords();

            if (resource.Get)
                subRoute.Operations.Add(new Operation(HttpMethod.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Response = resource.Name,
                    Parameters = path.Parameters
                });
            if (resource.Put)
                subRoute.Operations.Add(new Operation(HttpMethod.Put)
                {
                    Name = "update" + resource.Name,
                    Summary = $"update {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Body = resource.Name,
                    Parameters = path.Parameters
                });
            if (resource.Delete)
                subRoute.Operations.Add(new Operation(HttpMethod.Delete)
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