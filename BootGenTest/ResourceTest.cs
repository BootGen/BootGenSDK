using System.Collections.Generic;
using System.Linq;
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
            TestEntitySchema(resource.Schema);
        }

        private static void TestEntitySchema(Schema schema)
        {
            Assert.AreEqual(4, schema.Properties.Count);
            Assert.AreEqual("Name", schema.Properties[0].Name);
            Assert.AreEqual(BuiltInType.String, schema.Properties[0].Type);
            Assert.AreEqual("Value", schema.Properties[1].Name);
            Assert.AreEqual(BuiltInType.Int32, schema.Properties[1].Type);
            Assert.AreEqual("TimeStamp", schema.Properties[2].Name);
            Assert.AreEqual(BuiltInType.Int64, schema.Properties[2].Type);
            Assert.AreEqual("Ok", schema.Properties[3].Name);
            Assert.AreEqual(BuiltInType.Bool, schema.Properties[3].Type);
        }

        [TestMethod]
        public void TestListResource()
        {
            var resource = Resource.FromClass<List<Entity>>();
            Assert.IsTrue(resource.IsCollection);
            TestEntityResource(resource);
        }


        [TestMethod]
        public void TestComplexResource()
        {
            var resource = Resource.FromClass<Complex>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(0, resource.Resoursces.Count);
            Schema schema = resource.Schema;
            TestComplexSchema(schema);
        }

        private static void TestComplexSchema(Schema schema)
        {
            Assert.AreEqual("Complex", schema.Name);
            Property property = schema.Properties.Last();
            Assert.AreEqual("Entity", property.Name);
            Assert.AreEqual(BuiltInType.Object, property.Type);
            Assert.IsFalse(property.IsCollection);
            TestEntitySchema(property.Schema);
        }

        [TestMethod]
        public void TestNestedResource()
        {
            TestNestedResource(Resource.FromClass<Nested>());
        }

        private static void TestNestedResource(Resource resource)
        {
            Assert.AreEqual("Nested", resource.Name);
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(1, resource.Resoursces.Count);
            Resource nestedResource = resource.Resoursces.First();
            Assert.IsFalse(nestedResource.IsCollection);
            TestEntityResource(nestedResource);
            Property property = resource.Schema.Properties.Last();
            Assert.AreNotEqual("Entity", property.Name);
        }

        [TestMethod]
        public void TestDoubleNestedResource()
        {
            var resource = Resource.FromClass<DoubleNested>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(1, resource.Resoursces.Count);
            TestNestedResource(resource.Resoursces.First());
        }
        [TestMethod]
        public void TestIllegalNesting()
        {
            try
            {
                var resource = Resource.FromClass<IllegalNesting>();
            }
            catch (IllegalNestingException e)
            {
                Assert.IsNotNull(e);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void TestComplexListResource()
        {
            var resource = Resource.FromClass<ComplexList>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(0, resource.Resoursces.Count);
            Schema schema = resource.Schema;
            TestComplexListSchema(schema);
        }

        private static void TestComplexListSchema(Schema schema)
        {
            Assert.AreEqual("ComplexList", schema.Name);
            Property property = schema.Properties.Last();
            Assert.AreEqual("Entities", property.Name);
            Assert.AreEqual(BuiltInType.Object, property.Type);
            Assert.IsTrue(property.IsCollection);
            TestEntitySchema(property.Schema);
        }

        [TestMethod]
        public void TestNestedListResource()
        {
            var resource = Resource.FromClass<NestedList>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(1, resource.Resoursces.Count);
            Resource nestedResource = resource.Resoursces.First();
            Assert.IsTrue(nestedResource.IsCollection);
            TestEntityResource(nestedResource);
            Property property = resource.Schema.Properties.Last();
            Assert.AreNotEqual("Entities", property.Name);
        }

        [TestMethod]
        public void TestRecursive()
        {
            try
            {
                var resource = Resource.FromClass<Recursive>();
            }
            catch (RecursionException e)
            {
                Assert.IsNotNull(e);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void TestIndirectRecursion()
        {
            try
            {
                var resource = Resource.FromClass<IndirectRecursion>();
            }
            catch (RecursionException e)
            {
                Assert.IsNotNull(e);
                return;
            }
            Assert.Fail();
        }
        [TestMethod]
        public void TestNestedRecursive()
        {
            try
            {
                var resource = Resource.FromClass<Recursive>();
            }
            catch (RecursionException e)
            {
                Assert.IsNotNull(e);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void TestNestedIndirectRecursion()
        {
            try
            {
                var resource = Resource.FromClass<NestedIndirectRecursion>();
            }
            catch (RecursionException e)
            {
                Assert.IsNotNull(e);
                return;
            }
            Assert.Fail();
        }
        
        [TestMethod]
        public void TestTreeResource()
        {
            var resource = Resource.FromClass<Tree>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(0, resource.Resoursces.Count);
            Property property = resource.Schema.Properties[2];
            Assert.AreEqual("Entity", property.Name);
            Assert.AreEqual(BuiltInType.Object, property.Type);
            Assert.IsFalse(property.IsCollection);
            TestEntitySchema(property.Schema);
            TestComplexSchema(resource.Schema.Properties[3].Schema);
            TestComplexListSchema(resource.Schema.Properties[4].Schema);
        }
        class Entity
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public long TimeStamp { get; set; }
            public bool Ok { get; set; }
        }
        class Complex
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Entity Entity { get; set; }
        }
        class Nested
        {
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public Entity Entity { get; set; }
        }
        class DoubleNested
        {
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public Nested Nested { get; set; }
        }

        class IllegalNesting
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Nested Nested { get; set; }
        }

        class ComplexList
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public List<Entity> Entities { get; set; }
        }
        class NestedList
        {
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public List<Entity> Entities { get; set; }
        }
        class Recursive
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Recursive Entity { get; set; }
        }
        class NestedRecursive
        {
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public NestedRecursive Entity { get; set; }
        }

        class Tree
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Entity Entity { get; set; }
            public Complex Complex { get; set; }
            public ComplexList ComplexList { get; set; }
        }

        class IndirectRecursion
        {
            public IndirestRecursion2 IndirestRecursion2 { get; set; }
        }

        class IndirestRecursion2
        {
            public IndirectRecursion IndirestRecursion { get; set; }
        }
        class NestedIndirectRecursion
        {
            [Resource]
            public NestedIndirestRecursion2 NestedIndirestRecursion2 { get; set; }
        }

        class NestedIndirestRecursion2
        {
            [Resource]
            public NestedIndirectRecursion NestedIndirestRecursion { get; set; }
        }
    }
}
