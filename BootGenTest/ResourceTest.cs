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
            Assert.AreEqual(7, schema.Properties.Count);
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
            Assert.AreEqual("DateTime", schema.Properties[5].Name);
            Assert.AreEqual(BuiltInType.DateTime, schema.Properties[5].BuiltInType);
            Assert.IsTrue(schema.Properties[5].IsRequired);
            Assert.AreEqual("Weekday", schema.Properties[6].Name);
            Assert.AreEqual("Weekday", schema.Properties[6].EnumSchema.Name);
            Assert.AreEqual(BuiltInType.Enum, schema.Properties[6].BuiltInType);
            Assert.IsTrue(schema.Properties[6].IsRequired);
            Assert.AreEqual(7, schema.Properties[6].EnumSchema.Values.Count);
            Assert.AreEqual("Monday", schema.Properties[6].EnumSchema.Values.First());
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
        public void TestRecursive()
        {
            var resourceStore = new ResourceBuilder(new SchemaStore());
            var resource = resourceStore.FromClass<Recursive>();
            Assert.AreEqual(4, resource.Schema.Properties.Count);
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
            var r = api.AddResource<ComplexList>("complex");
            var seedStore = new SeedDataStore();
            seedStore.Add(r, new List<ComplexList>{ new ComplexList { Name = "My Name", Entities = new List<Entity> { new Entity{ Name = "Hello"} } }});
            Assert.AreEqual(2, api.Schemas.Count);
            Assert.AreEqual(1, seedStore.Get(api.Schemas[0]).Count);
            Assert.AreEqual(1, seedStore.Get(api.Schemas[1]).Count);
        }

        enum Weekday { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
        class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
            public long TimeStamp { get; set; }
            public bool Ok { get; set; }
            public DateTime DateTime { get; set; }
            public Weekday Weekday { get; set; }
        }
        class Complex
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public Entity Entity { get; set; }
        }

        class ComplexList
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public List<Entity> Entities { get; set; }
        }
        class Recursive
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public Recursive Entity { get; set; }
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
    }
}
