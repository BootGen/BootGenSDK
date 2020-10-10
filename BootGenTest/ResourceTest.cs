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
            var resourceStore = new ResourceStore();
            var entityResource = resourceStore.AddResource<Entity>();
            var childResource = resourceStore.AddResource<Entity>(parent: entityResource, parentName: "Parent");
            childResource.Name = "Child";
            childResource.PluralName = "Children";
            var api = new BootGenApi(resourceStore);
            Assert.AreEqual("Entity", entityResource.Name);
            Assert.AreEqual("Entities", entityResource.PluralName);
            Assert.AreEqual(childResource, entityResource.NestedResources.First());
            Assert.AreEqual("Child", childResource.Name);
            Assert.AreEqual("Children", childResource.PluralName);
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