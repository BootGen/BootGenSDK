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
            Assert.AreEqual(4, resource.Schema.Properties.Count);
            Assert.AreEqual("Name", resource.Schema.Properties[0].Name);
            Assert.AreEqual(BuiltInType.String, resource.Schema.Properties[0].Type);
            Assert.AreEqual("Value", resource.Schema.Properties[1].Name);
            Assert.AreEqual(BuiltInType.Int32, resource.Schema.Properties[1].Type);
            Assert.AreEqual("TimeStamp", resource.Schema.Properties[2].Name);
            Assert.AreEqual(BuiltInType.Int64, resource.Schema.Properties[2].Type);
            Assert.AreEqual("Ok", resource.Schema.Properties[3].Name);
            Assert.AreEqual(BuiltInType.Bool, resource.Schema.Properties[3].Type);
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
            public int Value { get; set; }
            public long TimeStamp { get; set; }
            public bool Ok { get; set; }
        }
        class Group
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public List<Entity> Entities { get; set; }
        }
    }
}
