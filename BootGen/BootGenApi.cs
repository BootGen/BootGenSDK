﻿using System;
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
        internal ClassStore ClassStore { get; }
        internal EnumStore EnumStore { get; }
        private TypeBuilder ClassBuilder { get; }
        internal ResourceStore ResourceStore { get; }
        private readonly ResourceBuilder resourceBuilder;
        public List<Resource> Resources => ResourceStore.Resources.ToList();
        public List<Controller> Controllers { get; } = new List<Controller>();
        public List<ClassModel> StoredClasses => ClassStore.Classes.Where(s => s.Persisted).ToList();
        public List<ClassModel> Classes => ClassStore.Classes.Concat(wrappedTypes).ToList();
        public List<ClassModel> ServerClasses => Classes.Where(p => p.Location != Location.ClientOnly).ToList();
        public List<ClassModel> ClientClasses => Classes.Where(p => p.Location != Location.ServerOnly).ToList();
        public List<ClassModel> CommonClasses => Classes.Where(p => p.Location == Location.Both).ToList();
        public List<EnumModel> Enums => EnumStore.Enums;
        private List<ClassModel> wrappedTypes = new List<ClassModel>();
        public List<Route> Routes { get; } = new List<Route>();

        public BootGenApi()
        {
            ClassStore = new ClassStore();
            EnumStore = new EnumStore();
            ResourceStore = new ResourceStore();
            resourceBuilder = new ResourceBuilder(ClassStore, EnumStore);
            ClassBuilder = new TypeBuilder(ClassStore, EnumStore);
            var permissionClass = ClassBuilder.FromType(typeof(UserPermission));
            Classes.First(s => s.Name == "PermissionToken").Location = Location.ServerOnly;
            foreach (var c in Classes)
            {
                c.Persisted = true;
                OnClassAdded(c);
            }
            permissionClass.Properties.First(p => p.Name == "Id").Location = Location.ServerOnly;
            permissionClass.Properties.First(p => p.Name == "PermissionToken").Location = Location.ServerOnly;
            permissionClass.Properties.First(p => p.Name == "PermissionTokenId").Location = Location.ServerOnly;
        }

        private static ClassModel CreatePivot(Resource parent, Resource resource, string pivotName)
        {
            Property idProperty = new Property
            {
                Name = "Id",
                BuiltInType = BuiltInType.Int32,
                IsRequired = true
            };
            var pivotClass = new ClassModel
            {
                Name = pivotName,
                Location = Location.ServerOnly,
                IdProperty = idProperty,
                Properties = new List<Property> {
                        idProperty,
                        new Property {
                            Name = parent.Class.Name + "Id",
                            BuiltInType = parent.Class.IdProperty.BuiltInType,
                            IsRequired = true
                        },
                        new Property {
                            Name = parent.Class.Name,
                            BuiltInType = BuiltInType.Object,
                            Class = parent.Class,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Class.Name + "Id",
                            BuiltInType = resource.Class.IdProperty.BuiltInType,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Class.Name,
                            BuiltInType = BuiltInType.Object,
                            Class = resource.Class,
                            IsRequired = true
                        }
                    }
            };
            return pivotClass;
        }

        public Resource AddResource<T>(string name, bool isReadonly = false, Resource parent = null, string pivotName = null)
        {
            var classCount = Classes.Count;
            Resource resource = resourceBuilder.FromClass<T>(parent);
            resource.IsReadonly = isReadonly;
            resource.PluralName = name;
            if (parent == null)
                ResourceStore.Add(resource);
            else
                parent.NestedResources.Add(resource);

            Routes.AddRange(resource.GetRoutes(ClassStore));
            if (pivotName != null)
            {
                ClassModel pivotClass = CreatePivot(parent, resource, pivotName);
                resource.Pivot = pivotClass;
                ClassStore.Add(pivotClass);
            }
            OnResourceAdded(resource);
            foreach (var c in Classes.Skip(classCount))
            {
                c.Persisted = true;
                OnClassAdded(c);
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
                    var property = ClassBuilder.GetProperty(param.ParameterType);
                    property.Name = param.Name;
                    controllerMethod.Parameters.Add(property);
                }

                TypeDescription responseType = ClassBuilder.GetProperty(method.ReturnType);
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
                            IsCollection = type.IsCollection
                        }
                    }
            };
            wrappedTypes.Add(c);
            return new TypeDescription
            {
                BuiltInType = BuiltInType.Object,
                IsCollection = false,
                Class = c
            };
        }

        HashSet<int> ProcessedClassIds = new HashSet<int>();
        private void OnResourceAdded(Resource resource)
        {
            if (resource.Pivot == null)
                AddEfRelations(resource);
        }
        private void OnClassAdded(ClassModel c)
        {
            if (!ProcessedClassIds.Contains(c.Id))
            {
                AddEfRelationsParentToChild(c);
            }
            AddEfRelationsChildToParent(c);
        }


        public static void AddEfRelations(Resource resource)
        {
            Resource parent = resource.ParentResource;
            if (parent == null)
                return;
            if (!resource.Class.Properties.Any(p => p.Name == parent.Class.Name))
            {
                Property referenceProperty = new Property
                {
                    Name = parent.Class.Name,
                    BuiltInType = BuiltInType.Object,
                    Class = parent.Class,
                    IsCollection = false,
                    IsRequired = true,
                    Location = Location.ServerOnly,
                    ParentReference = true
                };
                resource.Class.Properties.Add(referenceProperty);
            }

            if (!resource.Class.Properties.Any(p => p.Name == parent.Class.Name + "Id"))
                resource.Class.Properties.Add(new Property
                {
                    Name = parent.Class.Name + "Id",
                    BuiltInType = parent.Class.IdProperty.BuiltInType,
                    IsCollection = false,
                    IsRequired = true,
                    Location = Location.ServerOnly,
                    IsInternal = true
                });
        }


        public void AddEfRelationsParentToChild(ClassModel c)
        {
            ProcessedClassIds.Add(c.Id);
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
                        IsCollection = false,
                        IsRequired = true,
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
                            IsCollection = false,
                            Location = Location.Both,
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

    public class Method
    {
        public string Name { get; set; }
        public List<Property> Parameters { get; set; }
        public TypeDescription ReturnType { get; set; }
    }
}