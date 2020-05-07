using System.Collections.Generic;
using System.IO;
using System.Linq;
using BootGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scriban;

namespace BootGenTest
{
    [TestClass]
    public class RestTest
    {
        [TestMethod]
        public void TestSimpleApi()
        {
            var api = new BootGenApi();
            api.AddResource<Pet>();
            var restModel = new RestModel(api);
            restModel.Title = "Swagger Petstore";
            restModel.Licence = "MIT";
            restModel.Version = "1.0.0";
            restModel.Url = "http://petstore.swagger.io/v1";
            var scribanFilePath = "oas3template.sbn";
            var template = Template.Parse(File.ReadAllText(scribanFilePath), scribanFilePath);
            var renderedApi = template.Render(new { api = restModel });
            Assert.AreEqual(renderedApi, File.ReadAllText("simple-api.yml"));
        }

        [TestMethod]
        public void TestCollectionApi()
        {
            var api = new BootGenApi();
            api.AddResource<List<Pet>>();
            var restModel = new RestModel(api);
            restModel.Title = "Swagger Petstore";
            restModel.Licence = "MIT";
            restModel.Version = "1.0.0";
            restModel.Url = "http://petstore.swagger.io/v1";
            var scribanFilePath = "oas3template.sbn";
            var template = Template.Parse(File.ReadAllText(scribanFilePath), scribanFilePath);
            var renderedApi = template.Render(new { api = restModel });
            Assert.AreEqual(renderedApi, File.ReadAllText("collection-api.yml"));
            //File.WriteAllText("/home/agabor/Documents/BootGen/BootGenTest/collection-api.yml", renderedApi);
        }

        [TestMethod]
        public void TestNestedApi()
        {
            var api = new BootGenApi();
            api.AddResource<List<Owner>>();
            var restModel = new RestModel(api);
            restModel.Title = "Swagger Petstore";
            restModel.Licence = "MIT";
            restModel.Version = "1.0.0";
            restModel.Url = "http://petstore.swagger.io/v1";
            var scribanFilePath = "oas3template.sbn";
            var template = Template.Parse(File.ReadAllText(scribanFilePath), scribanFilePath);
            var renderedApi = template.Render(new { api = restModel });
            //Assert.AreEqual(renderedApi, File.ReadAllText("nested-api.yml"));
            File.WriteAllText("/home/agabor/Documents/BootGen/BootGenTest/nested-api.yml", renderedApi);
        }

        class Pet
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string Tag { get; set; }
        }
       
        class Address
        {
            public int ZipCode { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public int HouseNumber { get; set; }
        }

        class Owner
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public Address Address { get; set; }
            public List<string> PhoneNumbers  { get; set; }
            
            [Resource]
            public List<Pet> Pets  { get; set; }
        }
    }
}