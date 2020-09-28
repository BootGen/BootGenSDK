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
        }

        private static ClassModel CreatePivot(Resource parent, Resource resource)
        {
            var pivotClass = new ClassModel
            {
                Name = resource.Name + "Pivot",
                PluralName = resource.Name + "Pivots",
                Location = Location.ServerOnly,
                Properties = new List<Property> {
                        new Property {
                            Name = parent.Name + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = true
                        },
                        new Property {
                            Name = parent.Name,
                            BuiltInType = BuiltInType.Object,
                            Class = parent.Class,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Name + "Id",
                            BuiltInType = BuiltInType.Int32,
                            IsRequired = true
                        },
                        new Property {
                            Name = resource.Name,
                            BuiltInType = BuiltInType.Object,
                            Class = resource.Class,
                            IsRequired = true
                        }
                    }
            };
            return pivotClass;
        }

        public Resource AddResource<T>(string name = null, string pluralName = null, bool isReadonly = false, Resource parent = null, string parentName = null, bool manyToMany = false, bool authenticate = false)
        {
            if (parent?.ParentResource != null)
                throw new Exception("Only a single layer of resource nesting is supported.");
            ParentRelation parentRel = null;
            if (parent != null)
                parentRel = new ParentRelation(parent, parentName);
            var classCount = Classes.Count;
            Resource resource = resourceBuilder.FromClass<T>(parentRel);
            resource.Authenticate = authenticate;
            resource.IsReadonly = isReadonly;
            if (name != null)
            {
                resource.Name = name;
                resource.PluralName = pluralName ?? name + "s";
            }
            else
            {
                resource.Name = resource.Class.Name;
                resource.PluralName = pluralName ?? resource.Class.PluralName;
            }
            if (parent == null)
            {
                if (ResourceStore.RootResources.Any(r => r.Name == resource.Name))
                    throw new Exception($"A root resource with name \"{resource.Name}\" already exists.");
                ResourceStore.Add(resource);
            }
            else
            {
                
                if (parent.NestedResources.Any(r => r.Name == resource.Name))
                    throw new Exception($"A nested resource with name \"{resource.Name}\" already exists under \"{parent.Name}\".");
                parent.NestedResources.Add(resource);
            }
            Routes.AddRange(resource.GetRoutes(ClassStore));
            if (manyToMany)
            {
                ClassModel pivotClass = CreatePivot(parent, resource);
                resource.Pivot = pivotClass;
                ClassStore.Add(pivotClass);
            }

            var newClasses = Classes.Skip(classCount).ToList();
            foreach (var c in newClasses)
            {
                if (c.Properties.All(p => p.Name != "Id"))
                    c.Properties.Insert(0, new Property
                    {
                        Name = "Id",
                        BuiltInType = BuiltInType.Int32,
                        IsRequired = true,
                        IsClientReadonly = true
                    });
                c.Persisted = true;
            }
            OnResourceAdded(resource, parentRel);
            foreach (var c in newClasses)
            {
                OnClassAdded(c);
            }
            return resource;
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
                            IsCollection = type.IsCollection,
                            IsRequired = true
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
        private void OnResourceAdded(Resource resource, ParentRelation parent)
        {
            if (resource.Pivot == null)
                AddEfRelations(resource, parent);
        }
        private void OnClassAdded(ClassModel c)
        {
            if (!ProcessedClassIds.Contains(c.Id))
            {
                AddEfRelationsParentToChild(c);
            }
            AddEfRelationsChildToParent(c);
        }


        private static void AddEfRelations(Resource resource, ParentRelation parent)
        {
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

    public class Method
    {
        public string Name { get; set; }
        public HttpVerb Verb { get; set; }
        public List<Property> Parameters { get; set; }
        public TypeDescription ReturnType { get; set; }
    }

    internal class ParentRelation
    {
        public ParentRelation(Resource resource, string name = null)
        {
            Resource = resource;
            Name = name ?? resource.Name;
        }
        public string Name { get; set; }
        public Resource Resource { get; set; }
        internal Property ParentIdProperty { get; set; }
    }
}