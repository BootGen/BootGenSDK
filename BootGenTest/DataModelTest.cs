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
            Assert.AreEqual(5, dataModel.Classes.Count);

            var tagClass = dataModel.Classes.First(c => c.Name.Singular == "Tag");
            Assert.AreEqual(4, tagClass.Properties.Count);
            AssertHasProperty(tagClass, "Id", BuiltInType.Int);
            AssertHasProperty(tagClass, "Name", BuiltInType.String);
            AssertHasProperty(tagClass, "Color", BuiltInType.String);
            AssertHasManyToManyProperty(tagClass, "Tasks");

            var taskClass = dataModel.Classes.First(c => c.Name.Singular == "Task");
            Assert.AreEqual(12, taskClass.Properties.Count);
            AssertHasProperty(taskClass, "Id", BuiltInType.Int);
            AssertHasProperty(taskClass, "Title", BuiltInType.String);
            AssertHasProperty(taskClass, "Description", BuiltInType.String);
            AssertHasProperty(taskClass, "Created", BuiltInType.DateTime);
            AssertHasProperty(taskClass, "Updated", BuiltInType.DateTime);
            AssertHasProperty(taskClass, "DueDate", BuiltInType.DateTime);
            AssertHasProperty(taskClass, "Priority", BuiltInType.Int);
            AssertHasProperty(taskClass, "IsOpen", BuiltInType.Bool);
            AssertHasProperty(taskClass, "EstimatedHours", BuiltInType.Float);
            AssertHasManyToManyProperty(taskClass, "Tags");
            AssertHasProperty(taskClass, "User", BuiltInType.Object);
            AssertHasProperty(taskClass, "UserId", BuiltInType.Int);

            var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
            Assert.AreEqual(6, userClass.Properties.Count);
            AssertHasProperty(userClass, "Id", BuiltInType.Int);
            AssertHasProperty(userClass, "UserName", BuiltInType.String);
            AssertHasProperty(userClass, "Email", BuiltInType.String);
            AssertHasOneToManyProperty(userClass, "Tasks");
            AssertHasProperty(userClass, "Address", BuiltInType.Object);
            AssertHasProperty(userClass, "AddressId", BuiltInType.Int);


            var addressClass = dataModel.Classes.First(c => c.Name.Singular == "Address");
            Assert.AreEqual(5, addressClass.Properties.Count);
            AssertHasProperty(addressClass, "Id", BuiltInType.Int);
            AssertHasProperty(addressClass, "City", BuiltInType.String);
            AssertHasProperty(addressClass, "Street", BuiltInType.String);
            AssertHasProperty(addressClass, "Zip", BuiltInType.String);
            AssertHasProperty(addressClass, "HouseNumber", BuiltInType.String);

            var userResource = resourceCollection.RootResources.First(r => r.Class == userClass);
            Assert.AreEqual(0, userResource.AlternateResources.Count);
            Assert.AreEqual(1, userResource.NestedResources.Count);
            var taskResource = resourceCollection.RootResources.First(r => r.Class == taskClass);
            Assert.AreEqual(2, taskResource.AlternateResources.Count);
            Assert.AreEqual(1, taskResource.NestedResources.Count);
            var tagResource = resourceCollection.RootResources.First(r => r.Class == tagClass);
            Assert.AreEqual(1, tagResource.AlternateResources.Count);
            Assert.AreEqual(1, tagResource.NestedResources.Count);
            Assert.IsNotNull(tagResource.AlternateResources.First().Pivot);
        }

        [TestMethod]
        public void TestLoadRecursiveModel()
        {
            var data = JObject.Parse(File.ReadAllText("example_recursive_input.json"), new JsonLoadSettings { CommentHandling = CommentHandling.Load });
            var dataModel = new DataModel();
            dataModel.Load(data);
            var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
            Assert.AreEqual(4, userClass.Properties.Count);
            AssertHasProperty(userClass, "Id", BuiltInType.Int);
            AssertHasProperty(userClass, "UserName", BuiltInType.String);
            AssertHasProperty(userClass, "Email", BuiltInType.String);
            var property = AssertHasManyToManyProperty(userClass, "Friends");
            Assert.AreEqual(property, property.MirrorProperty);

            var resourceCollection = new ResourceCollection(dataModel);
            var pivotClass = dataModel.Classes.First(c => c.Name.Singular == "UserFriend");
            Assert.AreEqual(5, pivotClass.Properties.Count);
            AssertHasProperty(pivotClass, "Id", BuiltInType.Int);
            AssertHasProperty(pivotClass, "User", BuiltInType.Object);
            AssertHasProperty(pivotClass, "UsersId", BuiltInType.Int);
            AssertHasProperty(pivotClass, "Friend", BuiltInType.Object);
            AssertHasProperty(pivotClass, "FriendsId", BuiltInType.Int);
        }

        private void AssertHasProperty(ClassModel classModel, string propertyName, BuiltInType type) {
            Assert.IsNotNull(classModel.Properties.FirstOrDefault(p => p.Name == propertyName && p.BuiltInType == type), $"{classModel.Name}.{propertyName} -> {type} is missing.");
        }

        private void AssertHasOneToManyProperty(ClassModel classModel, string propertyName) {
            Assert.IsNotNull(classModel.Properties.FirstOrDefault(p => p.Name == propertyName && p.BuiltInType == BuiltInType.Object && p.IsCollection && !p.IsManyToMany), $"{classModel.Name}.{propertyName} is missing.");
        }

        private Property AssertHasManyToManyProperty(ClassModel classModel, string propertyName) {
            var property = classModel.Properties.FirstOrDefault(p => p.Name == propertyName && p.BuiltInType == BuiltInType.Object && p.IsCollection && p.IsManyToMany);
            Assert.IsNotNull(property, $"{classModel.Name}.{propertyName} is missing.");
            return property;
        }
    }
}