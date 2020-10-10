using System;
using System.Collections.Generic;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BootGenTest
{
    [TestClass]
    public class IdentifierTest
    {

        class User
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestResourceIds()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var Users = resourceStore.AddResource<User>();
            var api = new Api(resourceStore);
            Assert.AreEqual(3, Users.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Users.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Email, Name", GetPropertyList(Users.Class));
        }

        class User2
        {
            public Guid Id { get; set;}
            public string Email { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestResourceIdsGuid()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var Users = resourceStore.AddResource<User2>();
            var api = new Api(resourceStore);
            Assert.AreEqual(3, Users.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Guid, Users.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Email, Name", GetPropertyList(Users.Class));
        }

        class Issue
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public User Assignee { get; set; }
        }

        
        [TestMethod]
        public void TestParentId()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var Issues = resourceStore.AddResource<Issue>();
            var api = new Api(resourceStore);
            Assert.AreEqual(5, Issues.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Title, Description, Assignee, AssigneeId", GetPropertyList(Issues.Class));
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.PropertyWithName("AssigneeId").BuiltInType);
        }

        class Issue2
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public User2 Assignee { get; set; }
        }

        
        [TestMethod]
        public void TestParentIdGuid()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var Issues = resourceStore.AddResource<Issue2>();
            var api = new Api(resourceStore);
            Assert.AreEqual(5, Issues.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Title, Description, Assignee, AssigneeId", GetPropertyList(Issues.Class));
            Assert.AreEqual(BuiltInType.Guid, Issues.Class.PropertyWithName("AssigneeId").BuiltInType);
        }

        class User3
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public List<Issue3> Issues { get; set; }
        }
        class Issue3
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        
        [TestMethod]
        public void TestParentIdOneToMany()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var Users = resourceStore.AddResource<User3>();
            var Issues = resourceStore.AddResource<Issue3>();
            var api = new Api(resourceStore);
            Assert.AreEqual(5, Issues.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Title, Description, User3, User3Id", GetPropertyList(Issues.Class));
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.PropertyWithName("User3Id").BuiltInType);
        }

        class User4
        {
            public Guid Id { get; set;}
            public string Email { get; set; }
            public string Name { get; set; }
            public List<Issue3> Issues { get; set; }
        }

        
        [TestMethod]
        public void TestParentIdOneToManyGuid()
        {
            var resourceStore = new ResourceCollection(new DataModel());
            var Users = resourceStore.AddResource<User4>();
            var Issues = resourceStore.AddResource<Issue3>();
            var api = new Api(resourceStore);
            Assert.AreEqual(5, Issues.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Title, Description, User4, User4Id", GetPropertyList(Issues.Class));
            Assert.AreEqual(BuiltInType.Guid, Issues.Class.PropertyWithName("User4Id").BuiltInType);
        }

        private string GetPropertyList(ClassModel c)
        {
            return c.Properties.Select(p => p.Name).Aggregate( (a, b) => $"{a}, {b}");
        }
    }
}
