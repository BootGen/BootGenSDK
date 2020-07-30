using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public static class RouteBuilder
    {
        public static List<Route> GetRoutes(this Resource resource, ClassStore classStore)
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
                resource.PermissionRoute = permissonRoute;
                permissonRoute.Operations = new List<Operation>();
                permissonRoute.PathModel = subRoute.PathModel.Adding(new PathComponent { Name = "permissions" });
                result.Add(permissonRoute);
                AddPermissionOperations(resource, permissonRoute, classStore);
            }
            return result;
        }

        private static Route GetItemRoute(Resource resource, Path basePath)
        {
            var subRoute = new Route();
            string itemIdName = resource.ClassModel.Name.ToCamelCase() + "Id";
            Parameter idParameter = ConvertToParameter(resource.ClassModel.IdProperty);
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
                                Parameters = method.Parameters.Where(p => p.ClassModel == null).Select(ToParam).ToList(),
                                Body = method.Parameters.FirstOrDefault( p => p.ClassModel != null)?.ClassModel,
                                BodyIsCollection = method.Parameters.FirstOrDefault( p => p.ClassModel != null)?.IsCollection == true,
                                Response = method.ReturnType.ClassModel,
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
            Parameter parameter = ConvertToParameter(p);
            parameter.Kind = p.BuiltInType == BuiltInType.Object ? RestParamterKind.Body : RestParamterKind.Query;
            return parameter;
        }
        public static Parameter ConvertToParameter(Property property)
        {
            var oasProp = new Parameter { Name = property.Name.ToSnakeCase() };
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    oasProp.IsRequired = true;;
                    break;
                case BuiltInType.Int32:
                    oasProp.IsRequired = true;
                    break;
                case BuiltInType.Int64:
                    oasProp.IsRequired = true;
                    break;
                case BuiltInType.String:
                    oasProp.IsRequired = false;
                    break;
                case BuiltInType.Object:
                    oasProp.IsRequired = false;
                    break;
            }
            oasProp.BuiltInType = property.BuiltInType;
            oasProp.ClassModel = property.ClassModel;
            oasProp.IsCollection = property.IsCollection;

            return oasProp;
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
                    Response = resource.ClassModel,
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
                    Body = resource.ClassModel,
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
                    Name = "get" + resource.ClassModel.Name + "ById",
                    Summary = $"retrieve {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful query",
                    Response = resource.ClassModel,
                    Parameters = path.Parameters
                });
            if (resource.ItemPut)
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Put,
                    Name = "update" + resource.ClassModel.Name + "ById",
                    Summary = $"update {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful update",
                    Body = resource.ClassModel,
                    Parameters = path.Parameters
                });
            if (resource.ItemDelete)
                subRoute.Operations.Add(new Operation
                {
                    Verb = HttpVerb.Delete,
                    Name = "delete" + resource.ClassModel.Name + "ById",
                    Summary = $"delete {resourceName} resource",
                    SuccessCode = 200,
                    SuccessDescription = $"successful deletion",
                    Parameters = path.Parameters
                });
        }
        private static void AddPermissionOperations(Resource resource, Route route, ClassStore classStore)
        {
            string resourceName = resource.PluralName.ToWords();
            var path = route.PathModel;
            classStore.TryGetValue(typeof(UserPermission), out ClassModel userPermissionClass);
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Get,
                Name = "get" + resource.ClassModel.Name + "Permissions",
                Summary = $"get permissions for {resourceName}",
                SuccessCode = 200,
                SuccessDescription = $"successful query",
                Response = userPermissionClass,
                ResponseIsCollection = true,
                Parameters = path.Parameters
            });
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Post,
                Name = "set" + resource.ClassModel.Name + "Permission",
                Summary = $"set {resourceName} permission",
                SuccessCode = 200,
                SuccessDescription = $"successful update",
                Body = userPermissionClass,
                Parameters = path.Parameters
            });
            route.Operations.Add(new Operation
            {
                Verb = HttpVerb.Delete,
                Name = "delete" + resource.ClassModel.Name + "Permission",
                Summary = $"delete {resourceName} permission",
                SuccessCode = 200,
                SuccessDescription = $"successful deletion",
                Parameters = path.Parameters
            });
        }
    }
}