using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    internal static class RouteBuilder
    {
        public static List<Route> GetRoutes(this RootResource resource)
        {
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.Name.Plural.ToCamelCase();
            Path basePath = new Path().Adding(new PathComponent { Name = resourceName.ToKebabCase() });
            route.PathModel = basePath;
            result.Add(route);
            resource.Route = route;
            route.Operations = new List<Operation>();
            AddCollectionOperations(resource, route, basePath);
            Route subRoute = GetItemRoute(resource, basePath);
            result.Add(subRoute);
            resource.ItemRoute = subRoute;
            AddItemOperations(resource, subRoute);
            return result;
        }
        public static List<Route> GetRoutes(this NestedResource resource)
        {
            Path basePath = resource.ParentResource.ItemRoute.PathModel;
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.Name.Plural.ToCamelCase();
            basePath = basePath.Adding(new PathComponent { Name = resourceName.ToKebabCase() });
            route.PathModel = basePath;
            result.Add(route);
            resource.Route = route;
            route.Operations = new List<Operation>();
            AddCollectionOperations(resource, route, basePath);
            if (resource.Pivot != null)
            {
                Route subRoute = GetItemRoute(resource, basePath);
                result.Add(subRoute);
                resource.ItemRoute = subRoute;
                AddItemOperations(resource, subRoute);
            }
            return result;
        }

        private static Route GetItemRoute(Resource resource, Path basePath)
        {
            var subRoute = new Route();
            string itemIdName = resource.Name.Singular.ToCamelCase() + "Id";
            Parameter idParameter = ConvertToParameter(resource.Class.Properties.First(p => p.Name == "Id"));
            idParameter.Name = itemIdName;
            idParameter.Kind = RestParamterKind.Path;
            var itemPath = basePath.Adding(new PathComponent { Parameter = idParameter, Name = itemIdName });
            subRoute.PathModel = itemPath;
            subRoute.Operations = new List<Operation>();
            return subRoute;
        }

        public static Parameter ConvertToParameter(Property property)
        {
            if (property == null)
                return null;
            var oasProp = new Parameter { Name = property.Name.ToCamelCase() };
            oasProp.BuiltInType = property.BuiltInType;
            oasProp.Class = property.Class;
            oasProp.IsCollection = property.IsCollection;

            return oasProp;
        }

        private static void AddCollectionOperations(Resource resource, Route route, Path path)
        {
            string resourceName = resource.Name.Plural.ToWords();
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Get,
                Name = TypeScriptGenerator.GetListFunctionName(resource),
                Summary = $"retrieve list of {resourceName}",
                Response = resource.Class,
                ResponseIsCollection = true,
                SuccessCode = 200,
                SuccessDescription = $"successful query",
                Parameters = path.Parameters
            });
            if (!resource.IsReadonly)
                route.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Post,
                    Name = TypeScriptGenerator.AddFunctionName(resource),
                    Summary = $"add a new element to the collection",
                    Body = resource.Class,
                    BodyIsCollection = false,
                    SuccessCode = 200,
                    SuccessDescription = $"successful insertion",
                    Parameters = path.Parameters,
                    Response = resource.Class
                });
        }


        private static void AddItemOperations(Resource resource, Route subRoute)
        {
            string resourceName = resource.Name.Plural.ToWords();
            var path = subRoute.PathModel;
            if (resource is RootResource)
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Get,
                    Name = TypeScriptGenerator.GetItemFunctionName(resource),
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Response = resource.Class,
                    Parameters = path.Parameters
                });
            if (!resource.IsReadonly)
            {
                if (resource is RootResource)
                    subRoute.Operations.Add(new Operation
                    {
                        Verb = HttpVerb.Put,
                        Name = TypeScriptGenerator.UpdateFunctionName(resource),
                        Summary = $"update {resourceName} resource",
                        SuccessCode = 200,
                        SuccessDescription = $"successful update",
                        Body = resource.Class,
                        Parameters = path.Parameters
                    });
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Delete,
                    Name = TypeScriptGenerator.DeleteFunctionName(resource),
                    Summary = $"delete {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
            }
        }
    }
}