using System;
using System.Collections.Generic;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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
            var resourceCollection = new ResourceCollection(new DataModel());
            resourceCollection.Load(JObject.Parse("{\"users\":[{\"email\":\"\", \"name\":\"\"}]}"));
            var Users = resourceCollection.RootResources.First();
            var api = new Api(resourceCollection);
            Assert.AreEqual(3, Users.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Users.Class.IdProperty.BuiltInType);
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
            var resourceCollection = new ResourceCollection(new DataModel());
            resourceCollection.Load(JObject.Parse("{\"users\":[{\"email\":\"\", \"name\":\"\", \"issues\":[{\"title\":\"\",\"description\":\"\"}]}]}"));
            var Issues = resourceCollection.RootResources.First(r => r.Name.Singular == "Issue");
            var api = new Api(resourceCollection);
            Assert.AreEqual(5, Issues.Class.Properties.Count);
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.IdProperty.BuiltInType);
            Assert.AreEqual("Id, Title, Description, User, UserId", GetPropertyList(Issues.Class));
            Assert.AreEqual(BuiltInType.Int32, Issues.Class.PropertyWithName("UserId").BuiltInType);
        }

        private string GetPropertyList(ClassModel c)
        {
            return c.Properties.Select(p => p.Name).Aggregate( (a, b) => $"{a}, {b}");
        }
    }
}
