using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using BootGen.Core;

namespace BootGenTest;

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
        var dataModel = new DataModel();
        dataModel.Load(JObject.Parse("{\"users\":[{\"email\":\"\", \"name\":\"\"}]}"));
        var resourceCollection = new ResourceCollection(dataModel);
        var Users = resourceCollection.RootResources.First();
        Assert.AreEqual(3, Users.Class.Properties.Count);
        Assert.AreEqual(BuiltInType.Int, Users.Class.IdProperty.BuiltInType);
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
        var dataModel = new DataModel();
        dataModel.Load(JObject.Parse("{\"users\":[{\"email\":\"\", \"name\":\"\", \"issues\":[{\"title\":\"\",\"description\":\"\"}]}]}"));
        var resourceCollection = new ResourceCollection(dataModel);
        var Issues = resourceCollection.RootResources.First(r => r.Name.Singular == "Issue");
        Assert.AreEqual(5, Issues.Class.Properties.Count);
        Assert.AreEqual(BuiltInType.Int, Issues.Class.IdProperty.BuiltInType);
        Assert.AreEqual("Id, Title, Description, User, UserId", GetPropertyList(Issues.Class));
        Assert.AreEqual(BuiltInType.Int, Issues.Class.PropertyWithName("UserId").BuiltInType);
    }

    private string GetPropertyList(ClassModel c)
    {
        return c.Properties.Select(p => p.Name).Aggregate( (a, b) => $"{a}, {b}");
    }
}
