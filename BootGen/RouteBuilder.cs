using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public static class RouteBuilder
    {
        public static List<Route> GetRoutes(this Resource resource, SchemaStore schemaStore)
        {
            Path basePath = resource.ParentResource?.ItemRoute?.PathModel ?? resource.ParentResource?.Route?.PathModel ?? new Path();
            var result = new List<Route>();
            var route = new Route();
            string resourceName = resource.PluralName.ToCamelCase();
            basePath = basePath.Adding(new PathComponent { Name = resourceName });
            route.PathModel = basePath;
            result.Add(route);
            resource.Route = route;
            route.Operations = new List<Operation>();
            AddCollectionOperations(resource, route, basePath);
            Route subRoute = GetItemRoute(resource, basePath);
            result.Add(subRoute);
            resource.ItemRoute = subRoute;
            AddItemOperations(resource, subRoute);
            if (resource.HasPermissions)
            {
                var permissonRoute = new Route();
                permissonRoute.Operations = new List<Operation>();
                permissonRoute.PathModel = subRoute.PathModel.Adding(new PathComponent { Name = "permissions" });
                result.Add(permissonRoute);
                AddPermissionOperations(resource, permissonRoute, schemaStore);
            }
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
                            new Operation
                            {
                                Verb = HttpVerb.Post,
                                Name = method.Name.ToCamelCase(),
                                Parameters = method.Parameters.Where(p => p.Schema == null).Select(ToParam).ToList(),
                                Body = method.Parameters.FirstOrDefault( p => p.Schema != null)?.Schema,
                                BodyIsCollection = method.Parameters.FirstOrDefault( p => p.Schema != null)?.IsCollection == true,
                                Response = method.ReturnType.Schema,
                                ResponseIsCollection = method.ReturnType.IsCollection,
                                SuccessCode = 200,
                                SuccessDescription = method.Name + " success",
                                Summary = method.Name
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
            string resourceName = resource.PluralName.ToWords();
            if (resource.Get)
                route.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Get,
                    Name = "get" + resource.PluralName,
                    Summary = $"retrieve list of {resourceName}",
                    Response = resource.Schema,
                    ResponseIsCollection = true,
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Parameters = path.Parameters
                });
            if (resource.Post)
                route.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Post,
                    Name = "add" + resource.PluralName,
                    Summary = $"add a new element to the collection",
                    Body = resource.Schema,
                    BodyIsCollection = false,
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
        }


        private static void AddItemOperations(Resource resource, Route subRoute)
        {
            string resourceName = resource.PluralName.ToWords();
            var path = subRoute.PathModel;
            if (resource.ItemGet)
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Get,
                    Name = "get" + resource.Schema.Name + "ById",
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Response = resource.Schema,
                    Parameters = path.Parameters
                });
            if (resource.ItemPut)
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Put,
                    Name = "update" + resource.Schema.Name + "ById",
                    Summary = $"update {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Body = resource.Schema,
                    Parameters = path.Parameters
                });
            if (resource.ItemDelete)
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Delete,
                    Name = "delete" + resource.Schema.Name + "ById",
                    Summary = $"delete {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
        }
        private static void AddPermissionOperations(Resource resource, Route route, SchemaStore schemaStore)
        {
            string resourceName = resource.PluralName.ToWords();
            var path = route.PathModel;
            schemaStore.TryGetValue(typeof(UserPermission), out Schema userPermissionSchema);
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Get,
                Name = "get" + resource.Schema.Name + "Permissions",
                Summary = $"get permissions for {resourceName}",
                SuccessCode = 200,
                SuccessDescription = $"successful query",
                Response = userPermissionSchema,
                ResponseIsCollection = true,
                Parameters = path.Parameters
            });
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Post,
                Name = "set" + resource.Schema.Name + "Permission",
                Summary = $"set {resourceName} permission",
                SuccessCode = 200,
                SuccessDescription = $"successful update",
                Body = userPermissionSchema,
                Parameters = path.Parameters
            });
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Delete,
                Name = "delete" + resource.Schema.Name + "Permission",
                Summary = $"delete {resourceName} permission",
                SuccessCode = 200,
                SuccessDescription = $"successful deletion",
                Parameters = path.Parameters
            });
        }
    }
}