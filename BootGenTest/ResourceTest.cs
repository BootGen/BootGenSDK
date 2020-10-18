using System;
using System.Collections.Generic;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BootGenTest
{
    [TestClass]
    public class ResourceTest
    {
        [HasTimestamps]
        [PluralName("Entities")]
        class Entity
        {
            public string Name { get; set; }
            [ClientOnly]
            public List<Entity> Children { get; set; }
        }

        [TestMethod]
        public void TestEntityResource()
        {
            var resourceCollection = new ResourceCollection(new DataModel());
            var entityResource = resourceCollection.Add<Entity>();
            var childResource = entityResource.OneToMany<Entity>("Parent");
            childResource.Name = "Child";
            childResource.Name.Plural = "Children";
            var api = new Api(resourceCollection);
            Assert.AreEqual("Entity", entityResource.Name.Singular);
            Assert.AreEqual("Entities", entityResource.Name.Plural);
            Assert.AreEqual(childResource, entityResource.NestedResources.First());
            Assert.AreEqual("Child", childResource.Name.Singular);
            Assert.AreEqual("Children", childResource.Name.Plural);
            Assert.AreEqual(7, entityResource.Class.Properties.Count);
            Assert.AreEqual("Id", entityResource.Class.Properties[0].Name);
            Assert.AreEqual("Name", entityResource.Class.Properties[1].Name);
            Assert.AreEqual("Children", entityResource.Class.Properties[2].Name);
            Assert.AreEqual("Created", entityResource.Class.Properties[3].Name);
            Assert.AreEqual("Updated", entityResource.Class.Properties[4].Name);
            Assert.AreEqual("Parent", entityResource.Class.Properties[5].Name);
            Assert.AreEqual("ParentId", entityResource.Class.Properties[6].Name);
        }

        [Readonly]
        [Authenticate]
        [Generate(controller: true, serviceInterface: true, service: false)]
        [ControllerName("MyDummyController")]
        [ServiceName("MyDummyService")]
        class Dummy
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestResourceAttributes()
        {
            var resourceCollection = new ResourceCollection(new DataModel());
            var resource = resourceCollection.Add<Dummy>();
            Assert.IsTrue(resource.IsReadonly);
            Assert.IsTrue(resource.Authenticate);
            Assert.IsTrue(resource.GenerationSettings.GenerateController);
            Assert.IsTrue(resource.GenerationSettings.GenerateServiceInterface);
            Assert.IsFalse(resource.GenerationSettings.GenerateService);
            Assert.AreEqual("MyDummyController", resource.GenerationSettings.ControllerName);
            Assert.AreEqual("MyDummyService", resource.GenerationSettings.ServiceName);
        }
    }
}