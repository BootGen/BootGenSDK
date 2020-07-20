using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public static class RouteBuilder
    {
        public static List<Route> GetRoutes(this Resource resource)
        {
            Path basePath = resource.ParentResource?.ItemRoute?.PathModel ?? resource.ParentResource?.Route?.PathModel ?? new Path();
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.Name.ToCamelCase();
            basePath = basePath.Adding(new PathComponent { Name = resourceName });
            route.PathModel = basePath;
            result.Add(route);
            resource.Route = route; 
            route.Operations = new List<Operation>();
            AddCollectionOperations(resource, route, basePath);
            Route subRoute = GetItemRoute(resource, basePath);
            result.Add(subRoute);
            resource.ItemRoute = subRoute;
            AddItemOperations(resource, subRoute, subRoute.PathModel);
            return result;
        }

        private static Route GetItemRoute(Resource resource, Path basePath)
        {
            var subRoute = new Route();
            string itemIdName = resource.Schema.Name.ToCamelCase() + "Id";
            Parameter idParameter = resource.Schema.IdProperty.ConvertToParameter();
            idParameter.Name = itemIdName;
            idParameter.Kind = RestParamterKind.Path;
            var itemPath = basePath.Adding(new PathComponent { Parameter = idParameter, Name = itemIdName });
            subRoute.PathModel = itemPath;
            subRoute.Operations = new List<Operation>();
            return subRoute;
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
                            new Operation(HttpVerb.Post)
                            {
                                Name = method.Name.ToCamelCase(),
                                Parameters = method.Parameters.Select(ToParam).ToList(),
                                Response = method.ReturnType.Schema?.Name,
                                ResponseIsCollection = method.ReturnType.IsCollection,
                                SuccessCode = 200,
                                SuccessDescription = method.Name + " success"
                            }
                        }
                    };
                }
        }

        private static Parameter ToParam(Property p)
        {
            Parameter parameter = p.ConvertToParameter();
            parameter.Kind = p.BuiltInType == BuiltInType.Object ? RestParamterKind.Body : RestParamterKind.Query;
            return parameter;
        }
        private static void AddCollectionOperations(Resource resource, Route route, Path path)
        {
            string resourceName = resource.Name.ToWords();
            if (resource.Get)
                route.Operations.Add(new Operation(HttpVerb.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve list of {resourceName}",
                    Response = resource.Name,
                    ResponseIsCollection = true,
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Parameters = path.Parameters
                });
            if (resource.Post)
                route.Operations.Add(new Operation(HttpVerb.Post)
                {
                    Name = "add" + resource.Name,
                    Summary = $"add a new element to the collection",
                    Body = resource.Schema.Name,
                    BodyIsCollection = false,
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
        }


        private static void AddItemOperations(Resource resource, Route subRoute, Path path)
        {
            string resourceName = resource.Name.ToWords();

            if (resource.ItemGet)
                subRoute.Operations.Add(new Operation(HttpVerb.Get)
                {
                    Name = "get" + resource.Name,
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Response = resource.Name,
                    Parameters = path.Parameters
                });
            if (resource.ItemPut)
                subRoute.Operations.Add(new Operation(HttpVerb.Put)
                {
                    Name = "update" + resource.Name,
                    Summary = $"update {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Body = resource.Schema.Name,
                    Parameters = path.Parameters
                });
            if (resource.ItemDelete)
                subRoute.Operations.Add(new Operation(HttpVerb.Delete)
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