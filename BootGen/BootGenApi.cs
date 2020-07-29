using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class ResourceStore
    {
        public List<Resource> Resources { get; } = new List<Resource>();
        public void Add(Resource resource)
        {
            Resources.Add(resource);
        }
    }
    public class BootGenApi
    {
        public SchemaStore SchemaStore { get; }
        public ResourceStore ResourceStore { get; }
        private readonly ResourceBuilder resourceBuilder;
        public List<Resource> Resources => ResourceStore.Resources.ToList();
        public List<Controller> Controllers { get; } = new List<Controller>();
        public List<Schema> StoredSchemas => SchemaStore.Schemas.Where(s => s.Persisted).ToList();
        public List<Schema> Schemas => SchemaStore.Schemas.Concat(wrappedTypes).ToList();
        public List<EnumSchema> EnumSchemas => SchemaStore.EnumSchemas;
        private List<Schema> wrappedTypes = new List<Schema>();
        public List<Route> Routes { get; } = new List<Route>();

        public BootGenApi()
        {
            SchemaStore = new SchemaStore();
            ResourceStore = new ResourceStore();
            resourceBuilder = new ResourceBuilder(SchemaStore);
            var permissionSchema = SchemaStore.GetSchemaForResource(typeof(UserPermission));
            Schemas.First(s => s.Name == "PermissionToken").Location = Location.ServerOnly;
            foreach (var schema in Schemas)
            {
                schema.Persisted = true;
                OnSchemaAdded(schema);
            }
            permissionSchema.Properties.First(p => p.Name == "Id").Location = Location.ServerOnly;
            permissionSchema.Properties.First(p => p.Name == "PermissionToken").Location = Location.ServerOnly;
            permissionSchema.Properties.First(p => p.Name == "PermissionTokenId").Location = Location.ServerOnly;
        }

        private static Schema CreatePivot(Resource parent, Resource resource, string pivotName)
        {
            Property idProperty = new Property
            {
                Name = "Id",
                BuiltInType = BuiltInType.Int32,
                IsRequired = true
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
                            BuiltInType = parent.Schema.IdProperty.BuiltInType,
                            IsRequired = true
                        },
                        new Property {
                            Name = parent.Schema.Name,
                            BuiltInType = BuiltInType.Object,
                            Schema = parent.Schema,
                            Tags = new List<string> { "hasOne" },
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Schema.Name + "Id",
                            BuiltInType = resource.Schema.IdProperty.BuiltInType,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Schema.Name,
                            BuiltInType = BuiltInType.Object,
                            Schema = resource.Schema,
                            Tags = new List<string> { "hasOne" },
                            IsRequired = true
                        }
                    }
            };
            return pivotSchema;
        }

        public Resource AddResource<T>(string name, bool isReadonly = false, bool hasPermissions = false, bool usePermissions = false, Resource parent = null, string pivotName = null)
        {
            var schemaCount = Schemas.Count;
            Resource resource = resourceBuilder.FromClass<T>(parent);
            resource.Get = true;
            resource.Post = !isReadonly;
            resource.ItemDelete = !isReadonly;
            resource.ItemPut = pivotName == null && !isReadonly;
            resource.ItemGet = pivotName == null;
            resource.PluralName = name;
            resource.HasPermissions = hasPermissions;
            resource.UsePermissions = hasPermissions || usePermissions || parent?.HasPermissions == true;
            if (parent == null)
                ResourceStore.Add(resource);
            else
                parent.NestedResources.Add(resource);

            Routes.AddRange(resource.GetRoutes(SchemaStore));
            if (pivotName != null)
            {
                Schema pivotSchema = CreatePivot(parent, resource, pivotName);
                resource.Pivot = pivotSchema;
                SchemaStore.Add(pivotSchema);
            }
            OnResourceAdded(resource);
            foreach (var schema in Schemas.Skip(schemaCount))
            {
                schema.Persisted = true;
                OnSchemaAdded(schema);
            }
            return resource;
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

        HashSet<int> ProcessedSchemas = new HashSet<int>();
        private void OnResourceAdded(Resource resource)
        {
            if (resource.Pivot == null)
                AddEfRelations(resource);
        }
        private void OnSchemaAdded(Schema schema)
        {
            if (!ProcessedSchemas.Contains(schema.Id))
            {
                AddEfRelationsParentToChild(schema);
            }
            AddEfRelationsChildToParent(schema);
        }


        public static void AddEfRelations(Resource resource)
        {
            Resource parent = resource.ParentResource;
            if (parent == null)
                return;
            if (!resource.Schema.Properties.Any(p => p.Name == parent.Schema.Name))
            {
                Property referenceProperty = new Property
                {
                    Name = parent.Schema.Name,
                    BuiltInType = BuiltInType.Object,
                    Schema = parent.Schema,
                    IsCollection = false,
                    IsRequired = true,
                    Location = Location.ServerOnly
                };
                referenceProperty.Tags.Add("hasOne");
                referenceProperty.Tags.Add("parentReference");
                resource.Schema.Properties.Add(referenceProperty);
            }

            if (!resource.Schema.Properties.Any(p => p.Name == parent.Schema.Name + "Id"))
                resource.Schema.Properties.Add(new Property
                {
                    Name = parent.Schema.Name + "Id",
                    BuiltInType = parent.Schema.IdProperty.BuiltInType,
                    IsCollection = false,
                    IsRequired = true,
                    Location = Location.ServerOnly
                });
        }


        public void AddEfRelationsParentToChild(Schema schema)
        {
            ProcessedSchemas.Add(schema.Id);
            foreach (var property in schema.Properties)
            {
                if (property.Schema == null || !property.IsCollection || property.MirrorProperty != null)
                    continue;

                Property referenceProperty = property.Schema.Properties.FirstOrDefault(p => p.Name == schema.Name);
                if (referenceProperty == null)
                {
                    referenceProperty = new Property
                    {
                        Name = schema.Name,
                        BuiltInType = BuiltInType.Object,
                        Schema = schema,
                        IsCollection = false,
                        IsRequired = true,
                        Location = Location.ServerOnly
                    };
                    referenceProperty.Tags.Add("hasOne");
                    referenceProperty.Tags.Add("parentReference");
                    property.Schema.Properties.Add(referenceProperty);
                }
                referenceProperty.MirrorProperty = property;
                property.MirrorProperty = referenceProperty;

                if (!property.Schema.Properties.Any(p => p.Name == schema.Name + "Id"))
                    property.Schema.Properties.Add(new Property
                    {
                        Name = schema.Name + "Id",
                        BuiltInType = schema.IdProperty.BuiltInType,
                        IsCollection = false,
                        IsRequired = true
                    });
                AddEfRelationsParentToChild(property.Schema);
            }
        }

        public void AddEfRelationsChildToParent(Schema schema)
        {
            var properties = new List<Property>(schema.Properties);
            var propertyIdx = -1;
            foreach (var property in properties)
            {
                propertyIdx += 1;
                if (property.Schema == null)
                    continue;
                if (!property.IsCollection && !property.Tags.Contains("hasOne"))
                {
                    schema.Properties.Insert(propertyIdx + 1, new Property
                    {
                        Name = property.Name + "Id",
                        BuiltInType = property.Schema.IdProperty.BuiltInType,
                        IsCollection = false,
                        Location = Location.Both,
                        IsRequired = true
                    });
                    property.Tags.Add("hasOne");
                    property.Location = Location.ServerOnly;
                    propertyIdx += 1;
                    AddEfRelationsChildToParent(property.Schema);
                }
            }
        }
    }

    public class Method
    {
        public string Name { get; set; }
        public List<Property> Parameters { get; set; }
        public TypeDescription ReturnType { get; set; }
    }
}