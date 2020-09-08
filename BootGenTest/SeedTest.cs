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
            var api = new BootGenApi();
            var Users = api.AddResource<User>("Users");
            var seedStore = new SeedDataStore(api);
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

        class User2
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestSeedGuid()
        {
            var api = new BootGenApi();
            var Users = api.AddResource<User2>("Users");
            var seedStore = new SeedDataStore(api);
            seedStore.Add(Users, new List<User2> { new User2 {
                    Name = "Name",
                    Email = "Email"
                }
            });
            var record = seedStore.Get(Users.Class).First();
            Assert.AreEqual(3, record.Values.Count);
            Assert.IsTrue(record.Get("Id").StartsWith("Guid.Parse"));
            Assert.AreEqual("\"Name\"", record.Get("Name"));
            Assert.AreEqual("\"Email\"", record.Get("Email"));
        }
    }
}
