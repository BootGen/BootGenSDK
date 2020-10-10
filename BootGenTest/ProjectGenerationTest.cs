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
        class Pet
        {
            public string Name { get; set; }
            public PetKind Kind { get; set; }
        }
        class User
        {
            public string Name { get; set; }
            [Resource]
            public List<User> Friends { get; set; }
            [Resource]
            public List<Pet> Pets { get; set; } 
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
            BootGenApi api = CreateAPI();
            new OASGenerator("testOutput").RenderApi("", "restapi.yml", "templates/oas3template.sbn", "Friends With Pets", api);
            CompareWithSample("restapi.yml");
        }

        [TestMethod]
        public void GenerateTSClassTest()
        {
            BootGenApi api = CreateAPI();
            var model = api.Classes.First();
            var tsGenerator = new TypeScriptGenerator("testOutput");
            tsGenerator.NameSpace = "UsersWithFriends";
            tsGenerator.RenderClasses("", model => $"TS{model.Name}.txt", "templates/client/ts_model.sbn", new List<ClassModel> { model });
            CompareWithSample($"TS{model.Name}.txt");
            tsGenerator.RenderEnums("", e => $"TS{e.Name}.txt", "templates/client/ts_enum.sbn", api.Enums);
            CompareWithSample($"TS{api.Enums.First().Name}.txt");
        }

        [TestMethod]
        public void GenerateVuexTest()
        {
            BootGenApi api = CreateAPI();
            var model = api.Classes.First();
            var tsGenerator = new TypeScriptGenerator("testOutput");
            tsGenerator.NameSpace = "UsersWithFriends";
            tsGenerator.RenderApiClient($"", "vuex.txt", "templates/client/vuex.sbn", api);
            CompareWithSample("vuex.txt");
        }

        [TestMethod]
        public void GenerateCSClassTest()
        {
            BootGenApi api = CreateAPI();
            var model = api.Classes.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderClasses("", model => $"CS{model.Name}.txt", "templates/server/csharp_model.sbn", new List<ClassModel> { model });
            CompareWithSample($"CS{model.Name}.txt");
            aspNetCoreFunctions.RenderEnums("", e => $"CS{e.Name}.txt", "templates/server/csharp_enum.sbn", api.Enums);
            CompareWithSample($"CS{api.Enums.First().Name}.txt");
        }

        [TestMethod]
        public void GenerateResourceControllerTest()
        {
            BootGenApi api = CreateAPI();
            var resource = api.Resources.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceController.txt", "templates/server/resourceController.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceController.txt");
        }

        [TestMethod]
        public void GenerateResourceServiceTest()
        {
            BootGenApi api = CreateAPI();
            var resource = api.Resources.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceService.txt", "templates/server/resourceService.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceService.txt");
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceServiceInterface.txt", "templates/server/resourceServiceInterface.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceServiceInterface.txt");
        }

        [TestMethod]
        public void GenerateNestedResourceServiceTest()
        {
            BootGenApi api = CreateAPI();
            var resource = api.Resources.First().NestedResources.First();
            var aspNetCoreFunctions = new AspNetCoreGenerator("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceService.txt", "templates/server/resourceService.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceService.txt");
            aspNetCoreFunctions.RenderResources("", c => $"{c.Name}ResourceServiceInterface.txt", "templates/server/resourceServiceInterface.sbn", new List<Resource> { resource });
            CompareWithSample($"{resource.Name}ResourceServiceInterface.txt");
        }

        [TestMethod]
        public void ControllerTest()
        {
            var api = new BootGenApi(new ResourceStore());
            api.AddController<Authentication>();
            var aspNetCoreFunctions = new AspNetCoreGenerator("testOutput");
            aspNetCoreFunctions.NameSpace = "UsersWithFriends";
            aspNetCoreFunctions.RenderControllers("", c => $"{c.Name}Controller.txt", "templates/server/controller.sbn", api.Controllers);
            CompareWithSample($"{api.Controllers.First().Name}Controller.txt");
        }

        private static void CompareWithSample(string fileName)
        {
            Assert.AreEqual(File.ReadAllText(System.IO.Path.Combine("SampleOutput", fileName)), File.ReadAllText(System.IO.Path.Combine("testOutput", fileName)));
        }

        private static BootGenApi CreateAPI()
        {
            var resourceStore = new ResourceStore();
            var userResource = resourceStore.AddResource<User>();
            userResource.Authenticate = true;
            var friendResource = resourceStore.AddResource<User>(parent: userResource, manyToMany: true);
            friendResource.Name = "Friend";
            friendResource.Authenticate = true;
            var petResource = resourceStore.AddResource<Pet>(parent: userResource);
            petResource.Authenticate = true;
            var api = new BootGenApi(resourceStore);
            api.BaseUrl = "http://localhost/api";
            return api;
        }
    }
}