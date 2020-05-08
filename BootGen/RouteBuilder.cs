using System.Collections.Generic;

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
            route.Path = basePath.ToString();
            result.Add(route);
            route.Operations = new List<Operation>();
            if (resource.IsCollection)
            {
                AddCollectionOperations(resource, route, basePath);
                var subRoute = new Route();
                string itemIdName = resourceName + "Id";
                Parameter idParameter = resource.Schema.IdProperty.ConvertProperty<Parameter>();
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
                result.AddRange(subResource.GetRoutes(basePath));
            }
            return result;
        }

        private static void AddCollectionOperations(Resource resource, Route route, Path path)
        {
            string resourceName = resource.Name.ToWords();
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
            string resourceName = resource.Name.ToWords();
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
            string resourceName = resource.Name.ToWords();

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