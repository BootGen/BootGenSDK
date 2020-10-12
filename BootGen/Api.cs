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
        private ControllerCollection ControllerCollection { get; }
        public List<Resource> Resources => ResourceCollection.Resources;
        public List<Controller> Controllers => ControllerCollection?.Controllers ?? new List<Controller>();
        public DataModel DataModel => ResourceCollection.DataModel;
        public List<Route> Routes { get; } = new List<Route>();
        public string BaseUrl { get; set; }

        public Api(ResourceCollection resourceCollection, ControllerCollection controllerCollection = null)
        {
            ResourceCollection = resourceCollection;
            ControllerCollection = controllerCollection;
            foreach (var resource in Resources)
            {
                Routes.AddRange(resource.GetRoutes());
                if (resource.Pivot == null)
                    AddEfRelations(resource);
                else
                    DataModel.ClassCollection.Add(resource.Pivot);
            }
            
            foreach (var controller in Controllers)
                Routes.AddRange(controller.GetRoutes());
            
            foreach (var c in DataModel.StoredClasses)
            {
                if (!c.RelationsAreSetUp)
                {
                    AddEfRelationsParentToChild(c);
                }
                AddEfRelationsChildToParent(c);
            }
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
                if (property.Class == null || !property.IsCollection || property.MirrorProperty != null || property.Location == Location.ClientOnly)
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