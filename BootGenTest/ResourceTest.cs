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
            [Resource]
            public List<Entity> Children { get; set; }
        }

        [TestMethod]
        public void TestEntityResource()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var entityResource = resourceStore.AddResource<Entity>();
            var childResource = entityResource.AddResource<Entity>(parentName: "Parent");
            childResource.Name = "Child";
            childResource.Name.Plural = "Children";
            var api = new Api(resourceStore);
            Assert.AreEqual("Entity", entityResource.Name.Singular);
            Assert.AreEqual("Entities", entityResource.Name.Plural);
            Assert.AreEqual(childResource, entityResource.NestedResources.First());
            Assert.AreEqual("Child", childResource.Name.Singular);
            Assert.AreEqual("Children", childResource.Name.Plural);
            Assert.AreEqual(6, entityResource.Class.Properties.Count);
            Assert.AreEqual("Id", entityResource.Class.Properties[0].Name);
            Assert.AreEqual("Name", entityResource.Class.Properties[1].Name);
            Assert.AreEqual("Created", entityResource.Class.Properties[2].Name);
            Assert.AreEqual("Updated", entityResource.Class.Properties[3].Name);
            Assert.AreEqual("Parent", entityResource.Class.Properties[4].Name);
            Assert.AreEqual("ParentId", entityResource.Class.Properties[5].Name);
        }
    }
}