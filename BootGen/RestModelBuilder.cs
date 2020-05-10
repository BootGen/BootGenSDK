using System;
using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public static class RestModelBuilder
    {
        public static RestModel GetRestModel(this BootGenApi api)
        {
            var result = new RestModel();
            result.Schemas = api.Schemas.Select(ConvertSchema).ToList();
            result.Routes = new List<Route>();
            foreach (var resource in api.Resources)
            {
                result.Routes.AddRange(resource.GetRoutes(new Path()));
            }
            foreach (var controller in api.Controllers)
            {
                var path = new Path { new PathComponent { Name = controller.Name.ToKebabCase() } };
                foreach (var method in controller.Methods) {
                    result.Routes.Add(new Route {
                        Path = path.Adding(new PathComponent{ Name = method.Name.ToKebabCase() }).ToString(),
                        Operations = new List<Operation> {
                            new Operation(HttpMethod.Post)
                            {
                                Name = method.Name.ToCamelCase(),
                                Parameters = method.Parameters.Select(p => p.ConvertProperty<Parameter>()).ToList()
                            }
                        }
                    });
                }
            }
            return result;
        }

        private static OASSchema ConvertSchema(Schema schema)
        {
            return new OASSchema
            {
                Name = schema.Name,
                Properties = schema.Properties.Select(p => p.ConvertProperty<OASProperty>()).ToList()
            };
        }
    }
}