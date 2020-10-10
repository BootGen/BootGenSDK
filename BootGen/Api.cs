using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Api
    {
        internal ResourceCollection ResourceCollection { get; }
        public List<Resource> Resources => ResourceCollection.Resources.ToList();
        public List<Controller> Controllers { get; } = new List<Controller>();
        public DataModel DataModel => ResourceCollection.DataModel;
        public List<Route> Routes { get; } = new List<Route>();
        public string BaseUrl { get; set; }

        public Api(ResourceCollection resourceStore)
        {
            ResourceCollection = resourceStore;
            
            foreach (var resource in Resources)
            {
                Routes.AddRange(resource.GetRoutes());
                if (resource.Pivot == null)
                    AddEfRelations(resource);
                else
                    DataModel.ClassCollection.Add(resource.Pivot);
            }
            foreach (var c in DataModel.Classes)
            {
                if (!c.RelationsAreSetUp)
                {
                    AddEfRelationsParentToChild(c);
                }
                AddEfRelationsChildToParent(c);
            }
        }

        public Controller AddController<T>(bool authenticate = false)
        {
            var type = typeof(T);
            var controller = new Controller
            {
                Name = type.Name.Split('.').Last(),
                Methods = new List<Method>(),
                Authenticate = authenticate
            };
            foreach (var method in type.GetMethods())
            {
                var verb = HttpVerb.Post;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(GetAttribute)))
                    verb = HttpVerb.Get;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(PostAttribute)))
                    verb = HttpVerb.Post;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(PatchAttribute)))
                    verb = HttpVerb.Patch;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(PutAttribute)))
                    verb = HttpVerb.Put;
                if (method.CustomAttributes.Any(a => a.AttributeType == typeof(DeleteAttribute)))
                    verb = HttpVerb.Delete;
                var controllerMethod = new Method
                {
                    Name = method.Name,
                    Parameters = new List<Property>(),
                    Verb = verb
                };
                controller.Methods.Add(controllerMethod);
                foreach (var param in method.GetParameters())
                {
                    var property = DataModel.TypeBuilder.GetProperty(param.ParameterType);
                    property.Name = param.Name;
                    controllerMethod.Parameters.Add(property);
                }

                TypeDescription responseType = DataModel.TypeBuilder.GetProperty(method.ReturnType);
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
            var c = new ClassModel
            {
                Name = name,
                Properties = new List<Property>
                    {
                        new Property
                        {
                            Name = "Value",
                            BuiltInType = type.BuiltInType,
                            IsCollection = type.IsCollection,
                            IsRequired = true
                        }
                    }
            };
            DataModel.ClassCollection.Add(c);
            return new TypeDescription
            {
                BuiltInType = BuiltInType.Object,
                IsCollection = false,
                Class = c
            };
        }



        private static void AddEfRelations(Resource resource)
        {
            var parent = resource.ParentRelation;
            if (parent == null)
                return;
            if (!resource.Class.Properties.Any(p => p.Name == parent.Name))
            {
                var referenceProperty = new Property
                {
                    Name = parent.Name,
                    BuiltInType = BuiltInType.Object,
                    Class = parent.Resource.Class,
                    IsCollection = false,
                    IsRequired = true,
                    Location = Location.ServerOnly,
                    ParentReference = true
                };
                resource.Class.Properties.Add(referenceProperty);
            }
            else
            {
                var referenceProperty = resource.Class.Properties.First(p => p.Name == parent.Name);
                referenceProperty.ParentReference = true;
            }

            if (!resource.Class.Properties.Any(p => p.Name == parent.Name + "Id"))
            {
                Property property = new Property
                {
                    Name = parent.Name + "Id",
                    BuiltInType = BuiltInType.Int32,
                    IsCollection = false,
                    IsRequired = true,
                    IdReferenceToParent = parent.Resource
                };
                resource.Class.Properties.Add(property);
                parent.ParentIdProperty = property;
            }
            else
            {
                var property = resource.Class.Properties.First(p => p.Name == parent.Name + "Id");
                property.IdReferenceToParent = parent.Resource;
                parent.ParentIdProperty = property;
            }
        }

        private void AddEfRelationsParentToChild(ClassModel c)
        {
            c.RelationsAreSetUp = true;
            foreach (var property in c.Properties)
            {
                if (property.Class == null || !property.IsCollection || property.MirrorProperty != null)
                    continue;

                Property referenceProperty = property.Class.Properties.FirstOrDefault(p => p.Name == c.Name);
                if (referenceProperty == null)
                {
                    referenceProperty = new Property
                    {
                        Name = c.Name,
                        BuiltInType = BuiltInType.Object,
                        Class = c,
                        IsCollection = false,
                        IsRequired = true,
                        Location = Location.ServerOnly,
                        ParentReference = true
                    };
                    property.Class.Properties.Add(referenceProperty);
                }
                referenceProperty.MirrorProperty = property;
                property.MirrorProperty = referenceProperty;

                if (!property.Class.Properties.Any(p => p.Name == c.Name + "Id"))
                    property.Class.Properties.Add(new Property
                    {
                        Name = c.Name + "Id",
                        BuiltInType = c.IdProperty.BuiltInType,
                        IsRequired = true
                    });
                AddEfRelationsParentToChild(property.Class);
            }
        }

        public void AddEfRelationsChildToParent(ClassModel c)
        {
            var properties = new List<Property>(c.Properties);
            var propertyIdx = -1;
            foreach (var property in properties)
            {
                propertyIdx += 1;
                if (property.Class == null)
                    continue;
                if (!property.IsCollection && property.Class != null)
                {
                    if (!c.Properties.Any(p => p.Name == property.Name + "Id"))
                    {
                        c.Properties.Insert(propertyIdx + 1, new Property
                        {
                            Name = property.Name + "Id",
                            BuiltInType = property.Class.IdProperty.BuiltInType,
                            IsRequired = true
                        });
                        property.Location = Location.ServerOnly;
                        propertyIdx += 1;
                        AddEfRelationsChildToParent(property.Class);
                    }
                }
            }
        }
    }
}