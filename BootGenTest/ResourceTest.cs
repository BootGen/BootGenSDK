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
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<Entity>();
            Assert.IsFalse(resource.IsCollection);
            TestEntityResource(resource);
        }

        private static void TestEntityResource(Resource resource)
        {
            TestEntitySchema(resource.Schema);
        }

        private static void TestEntitySchema(Schema schema)
        {
            Assert.AreEqual(5, schema.Properties.Count);
            Assert.AreEqual("Id", schema.Properties[0].Name);
            Assert.IsTrue(schema.Properties[0].IsRequired);
            Assert.AreEqual("Name", schema.Properties[1].Name);
            Assert.IsFalse(schema.Properties[1].IsRequired);
            Assert.AreEqual(BuiltInType.String, schema.Properties[1].BuiltInType);
            Assert.AreEqual("Value", schema.Properties[2].Name);
            Assert.AreEqual(BuiltInType.Int32, schema.Properties[2].BuiltInType);
            Assert.IsTrue(schema.Properties[2].IsRequired);
            Assert.AreEqual("TimeStamp", schema.Properties[3].Name);
            Assert.AreEqual(BuiltInType.Int64, schema.Properties[3].BuiltInType);
            Assert.IsTrue(schema.Properties[3].IsRequired);
            Assert.AreEqual("Ok", schema.Properties[4].Name);
            Assert.AreEqual(BuiltInType.Bool, schema.Properties[4].BuiltInType);
            Assert.IsTrue(schema.Properties[4].IsRequired);
        }

        [TestMethod]
        public void TestListResource()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<List<Entity>>();
            Assert.IsTrue(resource.IsCollection);
            TestEntityResource(resource);
        }


        [TestMethod]
        public void TestComplexResource()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<Complex>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(0, resource.NestedResources.Count);
            Schema schema = resource.Schema;
            TestComplexSchema(schema);
        }

        private static void TestComplexSchema(Schema schema)
        {
            Assert.AreEqual("Complex", schema.Name);
            Property property = schema.Properties.Last();
            Assert.AreEqual("Entity", property.Name);
            Assert.AreEqual(BuiltInType.Object, property.BuiltInType);
            Assert.IsFalse(property.IsCollection);
            TestEntitySchema(property.Schema);
        }

        [TestMethod]
        public void TestNestedResource()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            TestNestedResource(resourceStore.FromClass<Nested>());
        }

        private static void TestNestedResource(Resource resource)
        {
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(1, resource.NestedResources.Count);
            Resource nestedResource = resource.NestedResources.First();
            Assert.IsFalse(nestedResource.IsCollection);
            TestEntityResource(nestedResource);
            Property property = resource.Schema.Properties.Last();
            Assert.AreNotEqual("Entity", property.Name);
        }

        [TestMethod]
        public void TestDoubleNestedResource()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<DoubleNested>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(1, resource.NestedResources.Count);
            TestNestedResource(resource.NestedResources.First());
        }
        [TestMethod]
        public void TestIllegalNesting()
        {
            try
            {
                var resourceStore = new ResourceBuilder(new SchemaStore());
                var resource = resourceStore.FromClass<IllegalNesting>();
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
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<ComplexList>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(0, resource.NestedResources.Count);
            Schema schema = resource.Schema;
            TestComplexListSchema(schema);
        }

        private static void TestComplexListSchema(Schema schema)
        {
            Assert.AreEqual("ComplexList", schema.Name);
            Property property = schema.Properties.Last();
            Assert.AreEqual("Entities", property.Name);
            Assert.AreEqual(BuiltInType.Object, property.BuiltInType);
            Assert.IsTrue(property.IsCollection);
            TestEntitySchema(property.Schema);
        }

        [TestMethod]
        public void TestNestedListResource()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<NestedList>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(1, resource.NestedResources.Count);
            Resource nestedResource = resource.NestedResources.First();
            Assert.IsTrue(nestedResource.IsCollection);
            TestEntityResource(nestedResource);
            Property property = resource.Schema.Properties.Last();
            Assert.AreNotEqual("Entities", property.Name);
        }

        [TestMethod]
        public void TestRecursive()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<Recursive>();
            Assert.AreEqual(4, resource.Schema.Properties.Count);
        }

        [TestMethod]
        public void TestNestedIndirectRecursion()
        {
            try
            {
                var resourceStore = new ResourceBuilder(new SchemaStore());
                var resource = resourceStore.FromClass<NestedIndirectRecursion>();
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
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<Tree>();
            Assert.IsFalse(resource.IsCollection);
            Assert.AreEqual(0, resource.NestedResources.Count);
            Property property = resource.Schema.Properties[3];
            Assert.AreEqual("Entity", property.Name);
            Assert.AreEqual(BuiltInType.Object, property.BuiltInType);
            Assert.IsFalse(property.IsCollection);
            TestEntitySchema(property.Schema);
            TestComplexSchema(resource.Schema.Properties[4].Schema);
            TestComplexListSchema(resource.Schema.Properties[5].Schema);
        }
        [TestMethod]
        public void TestDataSeed()
        {
            var api = new BootGenApi();
            api.AddResource("complex", new List<ComplexList>{ new ComplexList { Name = "My Name", Entities = new List<Entity> { new Entity{ Name = "Hello"} } }});
            Assert.AreEqual(2, api.Schemas.Count);
            Assert.AreEqual(1, api.Schemas[0].DataSeed.Count);
            Assert.AreEqual(1, api.Schemas[1].DataSeed.Count);
        }
        [TestMethod]
        public void TestDataSeedNestedResource()
        {
            var api = new BootGenApi();
            api.AddResource("nested", new List<NestedList>{ new NestedList { Name = "My Name", Entities = new List<Entity> { new Entity{ Name = "Hello"} } }});
            Assert.AreEqual(2, api.Schemas.Count);
            Assert.AreEqual(1, api.Schemas[0].DataSeed.Count);
            Assert.AreEqual(1, api.Schemas[1].DataSeed.Count);
        }
        class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
            public long TimeStamp { get; set; }
            public bool Ok { get; set; }
        }
        class Complex
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public Entity Entity { get; set; }
        }
        class Nested
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public Entity Entity { get; set; }
        }
        class DoubleNested
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public Nested Nested { get; set; }
        }

        class IllegalNesting
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public Nested Nested { get; set; }
        }

        class ComplexList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public List<Entity> Entities { get; set; }
        }
        class NestedList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public List<Entity> Entities { get; set; }
        }
        class Recursive
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public Recursive Entity { get; set; }
        }
        class NestedRecursive
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            [Resource]
            public NestedRecursive Entity { get; set; }
        }

        class Tree
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public Entity Entity { get; set; }
            public Complex Complex { get; set; }
            public ComplexList ComplexList { get; set; }
        }

        class IndirectRecursion
        {
            public int Id { get; set; }
            public IndirestRecursion2 IndirestRecursion2 { get; set; }
        }

        class IndirestRecursion2
        {
            public int Id { get; set; }
            public IndirectRecursion IndirestRecursion { get; set; }
        }
        class NestedIndirectRecursion
        {
            public int Id { get; set; }
            [Resource]
            public NestedIndirestRecursion2 NestedIndirestRecursion2 { get; set; }
        }

        class NestedIndirestRecursion2
        {
            public int Id { get; set; }
            [Resource]
            public NestedIndirectRecursion NestedIndirestRecursion { get; set; }
        }
    }
}
