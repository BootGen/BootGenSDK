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
            restModel.Url = "1.0.0";
            var scribanFilePath = "oas3template.sbn";
            var template = Template.Parse(File.ReadAllText(scribanFilePath), scribanFilePath);
            var tmp = template.Render(new { api = restModel });
        }

        class Pet
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string Tag { get; set; }
        }
    }
}