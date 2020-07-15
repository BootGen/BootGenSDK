using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class BootGenApi
    {
        public SchemaStore SchemaStore { get; }
        private readonly ResourceBuilder resourceBuilder;
        public List<Resource> Resources { get; } = new List<Resource>();
        public List<Controller> Controllers { get; } = new List<Controller>();
        public List<Schema> StoredSchemas => SchemaStore.Schemas;
        public List<Schema> Schemas => SchemaStore.Schemas.Concat(wrappedTypes).ToList();
        public List<EnumSchema> EnumSchemas => SchemaStore.EnumSchemas;
        private List<Schema> wrappedTypes = new List<Schema>();
        public List<Route> Routes { get; } = new List<Route>();

        public BootGenApi()
        {
            SchemaStore = new SchemaStore();
            resourceBuilder = new ResourceBuilder(SchemaStore);
        }
        public Resource AddResource<T>(string name, bool isReadonly = false, Resource parent = null)
        {
            var schemaCount = Schemas.Count;
            Resource resource = resourceBuilder.FromClass<T>(parent);
            resource.Get = true;
            resource.Put = !isReadonly;
            resource.Name = name;
            if (parent == null)
                Resources.Add(resource);
            else
                parent.NestedResources.Add(resource);
            Routes.AddRange(resource.GetRoutes());
            OnResourceAdded(resource);
            foreach (var schema in Schemas.Skip(schemaCount))
                OnSchemaAdded(schema);
            return resource;
        }

        private static Schema CreatePivot(Resource parent, Resource resource, string pivotName)
        {
            Property idProperty = new Property
            {
                Name = "Id",
                BuiltInType = BuiltInType.Int32
            };
            var pivotSchema = new Schema
            {
                Name = pivotName,
                Location = Location.ServerOnly,
                IdProperty = idProperty,
                Properties = new List<Property> {
                        idProperty,
                        new Property {
                            Name = parent.Schema.Name + "Id",
                            BuiltInType = parent.Schema.IdProperty.BuiltInType
                        },
                        new Property {
                            Name = parent.Schema.Name,
                            BuiltInType = BuiltInType.Object,
                            Schema = parent.Schema,
                            Tags = new List<string> { "hasOne" }
                        },
                        new Property {
                            Name = resource.Schema.Name + "Id",
                            BuiltInType = resource.Schema.IdProperty.BuiltInType
                        },
                        new Property {
                            Name = resource.Schema.Name,
                            BuiltInType = BuiltInType.Object,
                            Schema = resource.Schema,
                            Tags = new List<string> { "hasOne" }
                        }
                    }
            };
            return pivotSchema;
        }

        public Resource AddResourceCollection<T>(string name, bool isReadonly = false, Resource parent = null, string pivotName = null)
        {
            var schemaCount = Schemas.Count;
            Resource resource = resourceBuilder.FromClass<T>(parent);
            resource.Get = true;
            resource.Post = !isReadonly;
            resource.ItemDelete = !isReadonly;
            if (pivotName == null)
            {
                resource.ItemGet = true;
                resource.ItemPut = !isReadonly;
            }
            resource.Name = name;
            resource.IsCollection = true;
            if (parent == null)
                Resources.Add(resource);
            else
                parent.NestedResources.Add(resource);
           
            Routes.AddRange(resource.GetRoutes());
            if (pivotName != null)
            {
                Schema pivotSchema = CreatePivot(parent, resource, pivotName);
                resource.Pivot = pivotSchema;
                SchemaStore.Add(pivotSchema);
            }
            OnResourceAdded(resource);
            foreach (var schema in Schemas.Skip(schemaCount))
                OnSchemaAdded(schema);
            return resource;
        }

        protected virtual void OnResourceAdded(Resource resource)
        {
        }
        protected virtual void OnSchemaAdded(Schema schema)
        {
        }

        public Controller AddController<T>()
        {
            var type = typeof(T);
            var controller = new Controller
            {
                Name = type.Name.Split('.').Last(),
                Methods = new List<Method>()
            };
            foreach (var method in type.GetMethods())
            {
                var controllerMethod = new Method
                {
                    Name = method.Name,
                    Parameters = new List<Property>()
                };
                controller.Methods.Add(controllerMethod);
                foreach (var param in method.GetParameters())
                {
                    var property = new SchemaBuilder(SchemaStore).GetTypeDescription<Property>(param.ParameterType);
                    property.Name = param.Name;
                    property.IsRequired = param.ParameterType.IsValueType;
                    controllerMethod.Parameters.Add(property);
                }

                TypeDescription responseType = new SchemaBuilder(SchemaStore).GetTypeDescription<Property>(method.ReturnType);
                if (responseType.BuiltInType == BuiltInType.Object)
                {
                    controllerMethod.ReturnType = responseType;
                }
                else
                {
                    controllerMethod.ReturnType = WrapType(method.Name + "Response", responseType);
                }
            }
            Controllers.Add(controller);
            Routes.AddRange(controller.GetRoutes());
            return controller;
        }

        private TypeDescription WrapType(string name, TypeDescription type)
        {
            Schema schema = new Schema
            {
                Name = name,
                Properties = new List<Property>
                    {
                        new Property
                        {
                            Name = "Value",
                            BuiltInType = type.BuiltInType,
                            IsCollection = type.IsCollection
                        }
                    }
            };
            wrappedTypes.Add(schema);
            return new TypeDescription
            {
                BuiltInType = BuiltInType.Object,
                IsCollection = false,
                Schema = schema
            };
        }
    }

    public class Controller
    {
        public string Name { get; set; }
        public List<Method> Methods { get; set; }
    }

    public class Method
    {
        public string Name { get; set; }
        public List<Property> Parameters { get; set; }
        public TypeDescription ReturnType { get; set; }
    }
}