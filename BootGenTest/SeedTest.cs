using System;
using System.Collections.Generic;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BootGenTest
{
    [TestClass]
    public class SeedTest
    {

        class User
        {
            public string Email { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestSeed()
        {
            var resourceCollection = new ResourceCollection(new DataModel());
            var Users = resourceCollection.Add<User>();
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Add(Users, new List<User> { new User {
                    Name = "Name",
                    Email = "Email"
                }
            });
            var record = seedStore.Get(Users.Class).First();
            Assert.AreEqual(3, record.Values.Count);
            Assert.AreEqual("1", record.Get("Id"));
            Assert.AreEqual("\"Name\"", record.Get("Name"));
            Assert.AreEqual("\"Email\"", record.Get("Email"));
        }

    }
}
