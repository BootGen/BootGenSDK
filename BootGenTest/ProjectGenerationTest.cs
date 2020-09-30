using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BootGenTest
{
    [TestClass]
    public class ProjectGenerationTest
    {
        class Pet
        {
            public string Name { get; set; }
        }
        class User
        {
            public string Name { get; set; }
            [Resource]
            public List<User> Friends { get; set; }
            [Resource]
            public List<Pet> Pets { get; set; } 
        }

        [TestMethod]
        public void GenerateAPITest()
        {
            BootGenApi api = CreateAPI();
            new OASFunctions("testOutput").RenderApi("", "restapi.yml", "templates/oas3template.sbn", "http://localhost/api", "Friends With Pets", api);
            CompareWithSample("restapi.yml");
        }

        [TestMethod]
        public void GenerateClassTest()
        {
            BootGenApi api = CreateAPI();
            var model = api.Classes.First();
            var aspNetCoreFunctions = new AspNetCoreFunctions("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderClasses("", model => $"{model.Name}.txt", "templates/server/csharp_model.sbn", new List<ClassModel> { model });
            CompareWithSample($"{model.Name}.txt");
        }

        [TestMethod]
        public void GenerateControllerTest()
        {
            BootGenApi api = CreateAPI();
            var resource = api.Resources.First();
            var aspNetCoreFunctions = new AspNetCoreFunctions("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceController.txt", "templates/server/resourceController.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceController.txt");
        }

        private static void CompareWithSample(string fileName)
        {
            Assert.AreEqual(File.ReadAllText(System.IO.Path.Combine("SampleOutput", fileName)), File.ReadAllText(System.IO.Path.Combine("testOutput", fileName)));
        }

        private static BootGenApi CreateAPI()
        {
            var api = new BootGenApi();
            var UserResource = api.AddResource<User>(authenticate: true);
            api.AddResource<User>(name: "Friend", parent: UserResource, manyToMany: true, authenticate: true);
            api.AddResource<Pet>(parent: UserResource, authenticate: true);
            return api;
        }
    }
}