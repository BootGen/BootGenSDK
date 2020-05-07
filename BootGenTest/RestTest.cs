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
            var tmp = template.Render(new { api = restModel });
            Assert.AreEqual(tmp, File.ReadAllText("simple-api.yml"));
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
            var tmp = template.Render(new { api = restModel });
            Assert.AreEqual(tmp, File.ReadAllText("collection-api.yml"));
        }

        class Pet
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string Tag { get; set; }
        }
    }
}