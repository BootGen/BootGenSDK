using System;
using System.Collections.Generic;
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
            var data = JObject.Parse("{\"users\":[{\"email\":\"Email\",\"name\":\"Name\",\"address\":{\"city\":\"Budapest\",\"street\":\"Macko\",\"number\":\"6\"},\"pets\":[{\"name\":\"Ubul\",\"type\":0},{\"name\":\"Garfield\",\"type\":1}]}]}");
            var dataModel = new DataModel();
            var resourceCollection = new JsonResourceCollection(dataModel);
            resourceCollection.Load(data);
            var seedStore = new JsonSeedStore(resourceCollection);
            seedStore.Load(data);
            var Users = resourceCollection.RootResources.First(r => r.Name.Singular == "User");
            var Pets = resourceCollection.RootResources.First(r => r.Name.Singular == "Pet");
            var record = seedStore.Get(Users.Class).First();
            Assert.AreEqual(4, record.Values.Count);
            Assert.AreEqual("1", record.Get("Id"));
            Assert.AreEqual("\"Name\"", record.Get("Name"));
            Assert.AreEqual("\"Email\"", record.Get("Email"));
            Assert.AreEqual("1", record.Get("AddressId"));
            record = seedStore.Get(dataModel.Classes.First(c => c.Name == "Address")).First();
            Assert.AreEqual(4, record.Values.Count);
            Assert.AreEqual("1", record.Get("Id"));
            Assert.AreEqual("\"Budapest\"", record.Get("City"));
            Assert.AreEqual("\"Macko\"", record.Get("Street"));
            Assert.AreEqual("\"6\"", record.Get("Number"));
            record = seedStore.Get(Pets.Class).First();
            Assert.AreEqual(4, record.Values.Count);
            Assert.AreEqual("1", record.Get("Id"));
            Assert.AreEqual("\"Ubul\"", record.Get("Name"));
            Assert.AreEqual("0", record.Get("Type"));
        }

    }
}
