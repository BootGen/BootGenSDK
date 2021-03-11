using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BootGen
{
    public class Api
    {
        private ResourceCollection ResourceCollection { get; }
        public List<Resource> Resources => ResourceCollection.Resources;
        public List<RootResource> RootResources => ResourceCollection.RootResources;
        public List<NestedResource> NestedResources => ResourceCollection.RootResources.SelectMany(r => r.NestedResources).ToList();
        public DataModel DataModel => ResourceCollection.DataModel;
        public List<Route> Routes { get; } = new List<Route>();
        public string BaseUrl { get; set; }

        public Api(ResourceCollection resourceCollection)
        {
            ResourceCollection = resourceCollection;
            foreach (var resource in RootResources)
            {
                Routes.AddRange(resource.GetRoutes());
                foreach (var nestedResource in resource.NestedResources)
                {
                    DoNestingSanityChecks(nestedResource);
                    Routes.AddRange(nestedResource.GetRoutes());
                    if (nestedResource.Pivot == null)
                        AddEfRelations(nestedResource);
                    else
                        DataModel.ClassCollection.Add(nestedResource.Pivot);
                }
            }
            
            foreach (var c in DataModel.StoredClasses)
            {
                if (!c.RelationsAreSetUp)
                {
                    AddEfRelationsParentToChild(c);
                }
                AddEfRelationsChildToParent(c);
            }
        }

        private static void DoNestingSanityChecks(NestedResource nestedResource)
        {
            if (nestedResource.Pivot != null && nestedResource.RootResource == null)
            {
                throw new Exception($"{nestedResource.Name.Plural} is declared as a Many-To-Many nested resource on {nestedResource.ParentResource.Name.Plural}, but is does not have an associated root resource.");
            }
            if (nestedResource.RootResource != null && nestedResource.RootResource.Class != nestedResource.Class)
            {
                throw new Exception($"Type mismatch: ${nestedResource.Name.Plural} has type ${nestedResource.Class.Name}, but its associated root resource has type {nestedResource.RootResource.Class.Name}");
            }
        }

        private static void AddEfRelations(NestedResource resource)
        {
            var parent = resource.ParentRelation;
            if (!resource.Class.Properties.Any(p => p.Name == parent.Name))
            {
                var referenceProperty = new Property
                {
                    Name = parent.Name,
                    BuiltInType = BuiltInType.Object,
                    Class = parent.Resource.Class,
                    IsCollection = false,
                    IsRequired = true,
                    PropertyType = PropertyType.ServerOnly,
                    IsParentReference = true
                };
                resource.Class.Properties.Add(referenceProperty);
            }
            else
            {
                var referenceProperty = resource.Class.Properties.First(p => p.Name == parent.Name);
                referenceProperty.IsParentReference = true;
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
                if (property.Class == null || !property.IsCollection || property.MirrorProperty != null || property.PropertyType == PropertyType.Virtual)
                    continue;

                Property referenceProperty = property.Class.Properties.FirstOrDefault(p => p.Name == c.Name);
                if (referenceProperty == null)
                {
                    referenceProperty = new Property
                    {
                        Name = property.IsManyToMany ? c.Name.Plural : c.Name.Singular,
                        BuiltInType = BuiltInType.Object,
                        Class = c,
                        IsCollection = property.IsManyToMany,
                        IsRequired = true,
                        PropertyType = PropertyType.ServerOnly,
                        IsParentReference = true
                    };
                    property.Class.Properties.Add(referenceProperty);
                }
                referenceProperty.MirrorProperty = property;
                property.MirrorProperty = referenceProperty;

                if (!property.IsManyToMany && !property.Class.Properties.Any(p => p.Name == c.Name + "Id"))
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
                            IsRequired = property.Class.IsResource,
                            PropertyType = property.Class.IsResource ? PropertyType.Normal : PropertyType.ServerOnly
                        });
                        if (property.Class.IsResource)
                            property.PropertyType = PropertyType.ServerOnly;
                        propertyIdx += 1;
                        AddEfRelationsChildToParent(property.Class);
                    }
                }
            }
        }
    }
}