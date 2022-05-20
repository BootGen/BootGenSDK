using Microsoft.VisualStudio.TestTools.UnitTesting;
using BootGen;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BootGenTest
{
    [TestClass]
    public class DataModelTest
    {
        [TestMethod]
        public void TestLoadModel()
        { 
            var data = JObject.Parse(File.ReadAllText("example_input.json"));
            var dataModel = new DataModel();
            var settings = JObject.Parse(File.ReadAllText("example_input_settings.json")).ToObject<Dictionary<string, ClassSettings>>();
            dataModel.Load(data, settings);
            var resourceCollection = new ResourceCollection(dataModel);
            Assert.AreEqual(5, dataModel.Classes.Count);

            var tagClass = dataModel.Classes.First(c => c.Name.Singular == "Tag");
            Assert.AreEqual(4, tagClass.Properties.Count);
            Assert.IsTrue(tagClass.ReferredPlural);
            Assert.IsFalse(tagClass.ReferredSingle);
            AssertHasProperty(tagClass, "Id", BuiltInType.Int);
            AssertHasProperty(tagClass, "Name", BuiltInType.String);
            AssertHasProperty(tagClass, "Color", BuiltInType.String);
            AssertHasManyToManyProperty(tagClass, "Tasks");

            var taskClass = dataModel.Classes.First(c => c.Name.Singular == "Task");
            Assert.AreEqual(12, taskClass.Properties.Count);
            Assert.IsTrue(taskClass.ReferredPlural);
            Assert.IsFalse(taskClass.ReferredSingle);
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
            Assert.IsTrue(userClass.ReferredPlural);
            Assert.IsFalse(userClass.ReferredSingle);
            AssertHasProperty(userClass, "Id", BuiltInType.Int);
            AssertHasProperty(userClass, "UserName", BuiltInType.String);
            AssertHasProperty(userClass, "Email", BuiltInType.String);
            AssertHasOneToManyProperty(userClass, "Tasks");
            AssertHasProperty(userClass, "Address", BuiltInType.Object);
            AssertHasProperty(userClass, "AddressId", BuiltInType.Int);


            var addressClass = dataModel.Classes.First(c => c.Name.Singular == "Address");
            Assert.AreEqual(5, addressClass.Properties.Count);
            Assert.IsFalse(addressClass.ReferredPlural);
            Assert.IsTrue(addressClass.ReferredSingle);
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
            var data = JObject.Parse(File.ReadAllText("example_recursive_input.json"));
            var dataModel = new DataModel();
            var settings = JObject.Parse(File.ReadAllText("example_recursive_input_settings.json")).ToObject<Dictionary<string, ClassSettings>>();
            dataModel.Load(data, settings);
            var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
            Assert.AreEqual(4, userClass.Properties.Count);
            AssertHasProperty(userClass, "Id", BuiltInType.Int);
            AssertHasProperty(userClass, "UserName", BuiltInType.String);
            AssertHasProperty(userClass, "Email", BuiltInType.String);
            var property = AssertHasManyToManyProperty(userClass, "Friends");
            Assert.AreEqual(property.Class.Name, property.MirrorProperty.Class.Name);

            var resourceCollection = new ResourceCollection(dataModel);
            var pivotClass = dataModel.Classes.First(c => c.Name.Singular == "UserFriend");
            Assert.AreEqual(5, pivotClass.Properties.Count);
            AssertHasProperty(pivotClass, "Id", BuiltInType.Int);
            AssertHasProperty(pivotClass, "User", BuiltInType.Object);
            AssertHasProperty(pivotClass, "UsersId", BuiltInType.Int);
            AssertHasProperty(pivotClass, "Friend", BuiltInType.Object);
            AssertHasProperty(pivotClass, "FriendsId", BuiltInType.Int);
        }


        [TestMethod]
        public void TestLoadRecursiveModel2()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_recursive2.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            var userClass = dataModel.Classes.First(c => c.Name.Singular == "Pet");
            Assert.AreEqual(8, userClass.Properties.Count);
            AssertHasProperty(userClass, "Id", BuiltInType.Int);
            AssertHasProperty(userClass, "UserId", BuiltInType.Int);
            AssertHasProperty(userClass, "User", BuiltInType.Object);
            AssertHasProperty(userClass, "Name", BuiltInType.String);
            AssertHasProperty(userClass, "Species", BuiltInType.String);
            AssertHasProperty(userClass, "Pets", BuiltInType.Object);
            AssertHasProperty(userClass, "PetId", BuiltInType.Int);
            AssertHasProperty(userClass, "Pet", BuiltInType.Object);
        }

        [TestMethod]
        public void TestLoadRootObject()
        {

            var data = JObject.Parse(File.ReadAllText("example_input.json"));
            var dataModel = new DataModel();
            dataModel.LoadRootObject("App", data);
            var appClass = dataModel.Classes.First(c => c.Name.Singular == "App");
            AssertHasOneToManyProperty(appClass, "Users");
        }

        [TestMethod]
        public void TestWrongPluralization()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_plural.json"));
            try {
                var dataModel = new DataModel();
                dataModel.Load(data);
                Assert.Fail();
            } catch  (NamingException e) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.Message));
                Assert.AreEqual("users", e.SuggestedName);
                Assert.AreEqual("user", e.ActualName);
                data = data.RenamingArrays(e.ActualName, e.SuggestedName);
                var dataModel = new DataModel();
                dataModel.Load(data);
                var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
                Assert.AreEqual("User", userClass.Name.Singular);
            }
        }
        
        [TestMethod]
        public void TestWrongPluralization2()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_singular.json"));
            try {
                var dataModel = new DataModel();
                dataModel.Load(data);
                Assert.Fail();
            } catch  (NamingException e) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.Message));
                Assert.AreEqual("user", e.SuggestedName);
                Assert.AreEqual("users", e.ActualName);
                data = data.RenamingObjects(e.ActualName, e.SuggestedName);
                var dataModel = new DataModel();
                dataModel.Load(data);
                var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
                Assert.AreEqual("Users", userClass.Name.Plural);
            }
        }
        [TestMethod]
        public void TestInconsistentTypes()
        {
            try {
                var data = JObject.Parse(File.ReadAllText("example_input_inconsistent_types.json"));
                var dataModel = new DataModel();
                dataModel.Load(data);
                Assert.Fail();
            } catch  (FormatException e) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.Message));
            }
        }
        [TestMethod]
        public void TestInconsistentTypes2()
        {
            try {
                var data = JObject.Parse(File.ReadAllText("example_input_inconsistent_types_2.json"));
                var dataModel = new DataModel();
                dataModel.Load(data);
                Assert.Fail();
            } catch  (FormatException e) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.Message));
            }
        }
        
        [TestMethod]
        public void TestEmptyArray()
        { 
            var data = JObject.Parse(File.ReadAllText("example_input_empty_array.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
        }

        [TestMethod]
        public void TestEmptyType()
        { 
            var data = JObject.Parse(File.ReadAllText("example_input_empty_type.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            Assert.AreEqual(1, dataModel.Warnings.Count);
            var names = dataModel.Warnings[WarningType.EmptyType];
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual("Pet", names.First());
            Assert.IsTrue(dataModel.Classes.All(c => c.Name.Singular != "Pet"));
            Assert.IsTrue(dataModel.Classes.First(c => c.Name.Singular == "User").Properties.All(p => p.Class?.Name.Singular != "Pet"));
        }
        
        [TestMethod]
        public void TestInvalidArray()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_invalid_array.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            Assert.AreEqual(1, dataModel.Warnings.Count);
            var names = dataModel.Warnings[WarningType.PrimitiveArrayElement];
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual("pets", names.First());
        }
        
        [TestMethod]
        public void TestInvalidPropertyName()
        {
            try {
                var data = JObject.Parse(File.ReadAllText("example_input_invalid_property_name.json"));
                var dataModel = new DataModel();
                dataModel.Load(data);
                Assert.Fail();
            } catch  (FormatException e) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.Message));
            }
        }

        [TestMethod]
        public void TestPokemon() {
            var data = JObject.Parse(File.ReadAllText("pokedex.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            Assert.AreEqual(1, dataModel.Warnings.Count);
            var names = dataModel.Warnings[WarningType.PrimitiveArrayElement];
            Assert.AreEqual(3, names.Count);
            Assert.IsTrue(names.Contains("types"));
            Assert.IsTrue(names.Contains("multipliers"));
            Assert.IsTrue(names.Contains("weaknesses"));
        }
        

        [TestMethod]
        public void TestPrimitiveRoot() {
            var data = JObject.Parse(File.ReadAllText("example_input_primitive_root.json"));
            var dataModel = new DataModel();
            dataModel.LoadRootObject("app", data);
            Assert.AreEqual(1, dataModel.Warnings.Count);
            Assert.AreEqual(1, dataModel.Warnings[WarningType.PrimitiveRoot].Count);
            Assert.AreEqual("Number", dataModel.Warnings[WarningType.PrimitiveRoot].First());
        }
        [TestMethod]
        public void TestLoadModelSettings()
        { 
            var data = JObject.Parse(File.ReadAllText("example_input.json"));
            var dataModel = new DataModel();
            var settings = JObject.Parse(File.ReadAllText("example_input_settings2.json")).ToObject<Dictionary<string, ClassSettings>>();
            dataModel.Load(data, settings);
            AssertSettingsEqual(settings, dataModel.GetSettings());
            var resourceCollection = new ResourceCollection(dataModel);
            Assert.AreEqual(5, dataModel.Classes.Count);

            var tagClass = dataModel.Classes.First(c => c.Name.Singular == "Tag");
            Assert.AreEqual(4, tagClass.Properties.Count);
            Assert.IsTrue(tagClass.ReferredPlural);
            Assert.IsFalse(tagClass.ReferredSingle);
            AssertHasProperty(tagClass, "Id", BuiltInType.Int);
            AssertHasProperty(tagClass, "Name", BuiltInType.String);
            AssertHasProperty(tagClass, "Color", BuiltInType.String);
            AssertHasManyToManyProperty(tagClass, "Tasks");

            var taskClass = dataModel.Classes.First(c => c.Name.Singular == "Task");
            Assert.AreEqual(12, taskClass.Properties.Count);
            Assert.IsTrue(taskClass.ReferredPlural);
            Assert.IsFalse(taskClass.ReferredSingle);
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
            var tagProperty = taskClass.Properties.First(p => p.Name == "Tags");
            Assert.IsTrue(tagProperty.IsReadOnly);
            Assert.AreEqual("Super Tags", tagProperty.VisibleName);

            var userClass = dataModel.Classes.First(c => c.Name.Singular == "User");
            Assert.AreEqual(4, userClass.Properties.Count);
            Assert.IsTrue(userClass.ReferredPlural);
            Assert.IsFalse(userClass.ReferredSingle);
            AssertHasProperty(userClass, "Id", BuiltInType.Int);
            AssertHasProperty(userClass, "UserName", BuiltInType.String);
            AssertHasProperty(userClass, "Email", BuiltInType.String);
            AssertHasOneToManyProperty(userClass, "Tasks");


            var addressClass = dataModel.Classes.First(c => c.Name.Singular == "Address");
            Assert.AreEqual(5, addressClass.Properties.Count);
            Assert.IsFalse(addressClass.ReferredPlural);
            Assert.IsTrue(addressClass.ReferredSingle);
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

        private void AssertSettingsEqual(Dictionary<string, ClassSettings> settings1, Dictionary<string, ClassSettings> settings2)
        {
            foreach (var t in settings1) {
                if (!settings2.TryGetValue(t.Key, out var settings))
                    continue;
                Assert.IsTrue(t.Value.LeftEquals(settings));
            }
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