using Microsoft.VisualStudio.TestTools.UnitTesting;
using BootGen;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System;
using System.Linq;

namespace BootGenTest
{
    [TestClass]
    public class DataModelTest
    {
        [TestMethod]
        public void TestLoadModel()
        { 
            var data = JObject.Parse(File.ReadAllText("example_input.json"), new JsonLoadSettings { CommentHandling = CommentHandling.Load });
            var dataModel = new DataModel();
            dataModel.Load(data);
            var resourceCollection = new ResourceCollection(dataModel);
            var api = new Api(resourceCollection);
            Assert.AreEqual(4, dataModel.Classes.Count);

            var tagClass = dataModel.Classes.First(c => c.Name.Singular == "Tag");
            Assert.AreEqual(4, tagClass.Properties.Count);
            AssertHasProperty(tagClass, "Id", BuiltInType.Int32);
            AssertHasProperty(tagClass, "Name", BuiltInType.String);
            AssertHasProperty(tagClass, "Color", BuiltInType.String);
            AssertHasProperty(tagClass, "Tasks", BuiltInType.Object);

            var taskClass = dataModel.Classes.First(c => c.Name.Singular == "Task");
            Assert.AreEqual(6, taskClass.Properties.Count);
            AssertHasProperty(taskClass, "Id", BuiltInType.Int32);
            AssertHasProperty(taskClass, "Title", BuiltInType.String);
            AssertHasProperty(taskClass, "Description", BuiltInType.String);
            AssertHasProperty(taskClass, "Tags", BuiltInType.Object);
            AssertHasProperty(taskClass, "User", BuiltInType.Object);
            AssertHasProperty(taskClass, "UserId", BuiltInType.Int32);

            var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
            Assert.AreEqual(4, userClass.Properties.Count);
            AssertHasProperty(userClass, "Id", BuiltInType.Int32);
            AssertHasProperty(userClass, "UserName", BuiltInType.String);
            AssertHasProperty(userClass, "Email", BuiltInType.String);
            //AssertHasProperty(UserClass, "PasswordHash", BuiltInType.String);
            AssertHasProperty(userClass, "Tasks", BuiltInType.Object);

            var userResource = resourceCollection.RootResources.First(r => r.Class == userClass);
            Assert.AreEqual(0, userResource.AlternateResources.Count);
            var taskResource = resourceCollection.RootResources.First(r => r.Class == taskClass);
            Assert.AreEqual(2, taskResource.AlternateResources.Count);
            var tagResource = resourceCollection.RootResources.First(r => r.Class == tagClass);
            Assert.AreEqual(1, tagResource.AlternateResources.Count);
        }

        private void AssertHasProperty(ClassModel classModel, string propertyName, BuiltInType type) {
            Assert.IsNotNull(classModel.Properties.FirstOrDefault(p => p.Name == propertyName && p.BuiltInType == type), $"{classModel.Name}.{propertyName} -> {type} is missing.");
        }
    }
}