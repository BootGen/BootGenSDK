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
            public Address Address { get; set; }

            [ClientOnly]
            public List<Pet> Pets { get; set; }
        }

        class Address
        {
            public string City { get; set; }
            public string Street { get; set; }
            public string Number { get; set; }
        }

        enum PetType { Dog, Cat, Fish }

        class Pet
        {
            public string Name { get; set; }
            public PetType Type { get; set; }

        }

        [TestMethod]
        public void TestSeed()
        {
            var dataModel = new DataModel();
            var resourceCollection = new ResourceCollection(dataModel);
            var Users = resourceCollection.Add<User>();
            var Pets = Users.OneToMany<Pet>();
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Add(Users, new List<User> { new User {
                    Name = "Name",
                    Email = "Email",
                    Address = new Address {
                        City = "Budapest",
                        Street = "Macko",
                        Number = "6"
                    },
                    Pets = new List<Pet> {
                        new Pet {
                            Name = "Ubul",
                            Type = PetType.Dog
                        },
                        new Pet {
                            Name = "Garfield",
                            Type = PetType.Cat
                        }
                    }
                }
            });
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
            Assert.AreEqual("PetType.Dog", record.Get("Type"));
        }

    }
}
