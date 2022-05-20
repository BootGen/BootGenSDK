using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace BootGenTest
{
    [TestClass]
    public class SeedTest
    {

        [TestMethod]
        public void TestSeed()
        {
            var data = JObject.Parse(File.ReadAllText("example_input.json"));
            var dataModel = new DataModel();
            var settings = JObject.Parse(File.ReadAllText("example_input_settings.json")).ToObject<Dictionary<string, ClassSettings>>();
            dataModel.Load(data, settings);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
            var users = resourceCollection.RootResources.First(r => r.Name.Singular == "User");
            var tasks = resourceCollection.RootResources.First(r => r.Name.Singular == "Task");
            var tag = resourceCollection.RootResources.First(r => r.Name.Singular == "Tag");
            var userRecord = seedStore.Get(users.Class).First();
            Assert.AreEqual(4, userRecord.Values.Count);
            Assert.AreEqual("1", userRecord.Get("Id"));
            Assert.AreEqual("\"Test User\"", userRecord.Get("UserName"));
            Assert.AreEqual("\"example@email.com\"", userRecord.Get("Email"));
            Assert.AreEqual("1", userRecord.Get("AddressId"));
            
            var taskRecord = seedStore.Get(tasks.Class).First();
            Assert.AreEqual(10, taskRecord.Values.Count);
            Assert.AreEqual("1", taskRecord.Get("Id"));
            Assert.AreEqual("1", taskRecord.Get("UserId"));
            Assert.AreEqual("1", taskRecord.Get("Priority"));
            Assert.AreEqual("1.5f", taskRecord.Get("EstimatedHours"));
            Assert.AreEqual("true", taskRecord.Get("IsOpen"));
            Assert.AreEqual("new DateTime(2021, 12, 30, 12, 0, 5)", taskRecord.Get("DueDate"));
            Assert.AreEqual("DateTime.Now", taskRecord.Get("Updated"));
            Assert.AreEqual("DateTime.Now", taskRecord.Get("Created"));
            Assert.AreEqual("\"Task Title\"", taskRecord.Get("Title"));
            Assert.AreEqual("\"Task description\"", taskRecord.Get("Description"));

            var tagRecord = seedStore.Get(tag.Class).First();
            Assert.AreEqual(3, tagRecord.Values.Count);
            Assert.AreEqual("1", tagRecord.Get("Id"));
            Assert.AreEqual("\"important\"", tagRecord.Get("Name"));
            Assert.AreEqual("\"red\"", tagRecord.Get("Color"));

            var pivotRecord = seedStore.Get(dataModel.Classes.First(c => c.Name.Singular == "TagTask")).First();
            Assert.AreEqual(2, pivotRecord.Values.Count);
            Assert.AreEqual("1", pivotRecord.Get("TagsId"));
            Assert.AreEqual("1", pivotRecord.Get("TasksId"));
        }

        [TestMethod]
        public void TestSeed2()
        {
            var data = JObject.Parse(File.ReadAllText("example_recursive_input.json"));
            var dataModel = new DataModel();
            var settings = JObject.Parse(File.ReadAllText("example_recursive_input_settings.json")).ToObject<Dictionary<string, ClassSettings>>();
            dataModel.Load(data, settings);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
            var users = resourceCollection.RootResources.First(r => r.Name.Singular == "User");
            var seedRecords = seedStore.Get(users.Class);
            Assert.AreEqual(2, seedRecords.Count);
            var userRecord = seedRecords.First();
            Assert.AreEqual(3, userRecord.Values.Count);
            Assert.AreEqual("1", userRecord.Get("Id"));
            Assert.AreEqual("\"Test User\"", userRecord.Get("UserName"));
            Assert.AreEqual("\"example@email.com\"", userRecord.Get("Email"));

            userRecord = seedRecords.Last();
            Assert.AreEqual(3, userRecord.Values.Count);
            Assert.AreEqual("2", userRecord.Get("Id"));
            Assert.AreEqual("\"Test User 2\"", userRecord.Get("UserName"));
            Assert.AreEqual("\"example2@email.com\"", userRecord.Get("Email"));
        }
        


        [TestMethod]
        public void TestLoad()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_3_users.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
        }

        [TestMethod]
        public void TestLoad2()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_single.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
        }

        [TestMethod]
        public void TestRecursion()
        {
            var data = JObject.Parse(File.ReadAllText("example_input_direct_recursion.json"));
            var dataModel = new DataModel();
            dataModel.Load(data);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
        }
        
    }
}
