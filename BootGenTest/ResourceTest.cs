using System.Collections.Generic;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BootGenTest
{
    [TestClass]
    public class ResourceTest
    {
        [TestMethod]
        public void TestSimpleResource()
        {
            var resource = Resource.FromClass<Entity>();
            Assert.IsFalse(resource.IsCollection);
            TestEntityResource(resource);
        }

        private static void TestEntityResource(Resource resource)
        {
            Assert.AreEqual("Entity", resource.Name);
            Assert.AreEqual(2, resource.Schema.Properties.Count);
            Assert.AreEqual("Name", resource.Schema.Properties[0].Name);
            Assert.AreEqual(BuiltInType.String, resource.Schema.Properties[0].Type);
            Assert.AreEqual("Value", resource.Schema.Properties[1].Name);
            Assert.AreEqual(BuiltInType.String, resource.Schema.Properties[1].Type);
        }

        [TestMethod]
        public void TestListResource()
        {
            var resource = Resource.FromClass<List<Entity>>();
            Assert.IsTrue(resource.IsCollection);
            TestEntityResource(resource);
        }

        class Entity
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        class Group
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public List<Entity> Entities { get; set; }
        }
    }
}
