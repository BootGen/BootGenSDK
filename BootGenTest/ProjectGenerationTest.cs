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
        enum PetKind { Dog, Cat, Bird, Fish }
        [Authenticate]
        class Pet
        {
            public string Name { get; set; }
            public PetKind Kind { get; set; }
        }
        [Authenticate]
        class User
        {
            public string Name { get; set; }
            [OneToMany]
            public List<Pet> Pets { get; set; } 
            [ManyToMany("UserPivot")]
            public List<User> Friends { get; set; }
        }
            class AuthenticationData
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    class LoginResponse
    {
        public string Jwt { get; set; }
        public User User { get; set; }
    }

    interface Authentication
    {
        LoginResponse Login(AuthenticationData data);
    }

        [TestMethod]
        public void GenerateAPITest()
        {
            Api api = CreateAPI();
            new OASGenerator(new Disk("testOutput")).RenderApi("", "restapi.yml", "templates/oas3template.sbn", "Friends With Pets", api);
            CompareWithSample("restapi.yml");
        }

        [TestMethod]
        public void GenerateTSClassTest()
        {
            Api api = CreateAPI();
            var model = api.DataModel.Classes.First();
            var tsGenerator = new TypeScriptGenerator(new Disk("testOutput"));
            tsGenerator.NameSpace = "UsersWithFriends";
            tsGenerator.RenderClasses("", model => $"TS{model.Name}.txt", "templates/client/ts_model.sbn", new List<ClassModel> { model });
            CompareWithSample($"TS{model.Name}.txt");
            tsGenerator.RenderEnums("", e => $"TS{e.Name}.txt", "templates/client/ts_enum.sbn", api.DataModel.Enums);
            CompareWithSample($"TS{api.DataModel.Enums.First().Name}.txt");
        }

        [TestMethod]
        public void GenerateVuexTest()
        {
            Api api = CreateAPI();
            var model = api.DataModel.Classes.First();
            var tsGenerator = new TypeScriptGenerator(new Disk("testOutput"));
            tsGenerator.NameSpace = "UsersWithFriends";
            //tsGenerator.RenderApiClient($"", "vuex.txt", "templates/client/vuex.sbn", api);
            //CompareWithSample("vuex.txt");
        }

        [TestMethod]
        public void GenerateCSClassTest()
        {
            Api api = CreateAPI();
            var model = api.DataModel.Classes.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator(new Disk("testOutput"));
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderClasses("", model => $"CS{model.Name}.txt", "templates/server/csharp_model.sbn", new List<ClassModel> { model });
            CompareWithSample($"CS{model.Name}.txt");
            aspNetCoreFunctions.RenderEnums("", e => $"CS{e.Name}.txt", "templates/server/csharp_enum.sbn", api.DataModel.Enums);
            CompareWithSample($"CS{api.DataModel.Enums.First().Name}.txt");
        }

        [TestMethod]
        public void GenerateResourceControllerTest()
        {
            Api api = CreateAPI();
            var resource = api.Resources.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator(new Disk("testOutput"));
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceController.txt", "templates/server/resourceController.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceController.txt");
        }

        [TestMethod]
        public void GenerateResourceServiceTest()
        {
            Api api = CreateAPI();
            var resource = api.Resources.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator(new Disk("testOutput"));
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceService.txt", "templates/server/resourceService.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceService.txt");
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceServiceInterface.txt", "templates/server/resourceServiceInterface.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceServiceInterface.txt");
        }

        [TestMethod]
        public void GeneratePivotResourceServiceTest()
        {
            Api api = CreateAPI();
            var resource = api.RootResources.First(r => r.Name.Singular == "User").NestedResources.First(r => r.Name.Singular == "Friend");
            var aspNetCoreFunctions = new AspNetCoreGenerator(new Disk("testOutput"));
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderClasses("", c => $"FriendPivotService.txt", "templates/server/pivotService.sbn", new List<ClassModel> { resource.Pivot });
            CompareWithSample($"FriendPivotService.txt");
            aspNetCoreFunctions.RenderClasses("", c => $"FriendPivotServiceInterface.txt", "templates/server/pivotServiceInterface.sbn", new List<ClassModel> { resource.Pivot });
            CompareWithSample($"FriendPivotServiceInterface.txt");
        }

        [TestMethod]
        public void GenerateNestedResourceServiceTest()
        {
            Api api = CreateAPI();
            var resource = api.RootResources.First(r => r.Name.Singular == "User").NestedResources.First(r => r.Name.Singular == "Pet");
            var generator = new AspNetCoreGenerator(new Disk("testOutput"));
            generator.NameSpace = "UsersWithFriends";
            generator.RenderResources("", c => $"{c.Name}ResourceService.txt", "templates/server/resourceService.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceService.txt");
            generator.RenderResources("", c => $"{c.Name}ResourceServiceInterface.txt", "templates/server/resourceServiceInterface.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceServiceInterface.txt");
        }

        private static void CompareWithSample(string fileName)
        {
            Assert.AreEqual(File.ReadAllText(System.IO.Path.Combine("SampleOutput", fileName)), File.ReadAllText(System.IO.Path.Combine("testOutput", fileName)));
        }

        private static Api CreateAPI()
        {
            var resourceCollection = new ResourceCollection(new DataModel());
            var userResource = resourceCollection.Add<User>();
            userResource.Authenticate = true;
            var api = new Api(resourceCollection);
            api.BaseUrl = "http://localhost/api";
            return api;
        }
    }
}