using Microsoft.VisualStudio.TestTools.UnitTesting;
using BootGen;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System;
using System.Linq;

namespace BootGenTest
{
    [TestClass]
    public class ProjectTest
    {
        [TestMethod]
        public void TestGenerate()
        {
            var data = JObject.Parse("{\"users\":[{\"email\":\"Email\",\"name\":\"Name\",\"address\":{\"city\":\"Budapest\",\"street\":\"Macko\",\"number\":\"6\"},\"pets\":[{\"name\":\"Ubul\",\"type\":0},{\"name\":\"Garfield\",\"type\":1}]}]}");
            var dataModel = new DataModel();
            var resourceCollection = new JsonResourceCollection(dataModel);
            resourceCollection.Load(data);
            var seedStore = new JsonSeedStore(resourceCollection);
            seedStore.Load(data);
            var disk = new VirtualDisk();
            var project = new Project
            {
                ControllerFolder = "Controllers",
                ServiceFolder = "Services",
                ClientFolder = "ClientApp/src",
                Disk = disk,
                ResourceCollection = resourceCollection,
                SeedStore = seedStore,
                Api = new Api(resourceCollection),
                TemplateRoot = "templates"
            };
            project.GenerateFiles("TestProject", "TestProject", "http://localhost:5000");
            if (Directory.Exists("SampleOutput"))
                Directory.Delete("SampleOutput", true);
            ZipFile.ExtractToDirectory("SampleOutput.zip", ".");
            foreach (var file in disk.Files) {
               var path = System.IO.Path.Combine("SampleOutput", file.Path, file.Name);
                var expectedLines = File.ReadAllLines(path);
                var actualLines = file.Content.Split(Environment.NewLine);
                for (int i = 0; i < expectedLines.Length; ++i) {
                    Assert.AreEqual(expectedLines[i], actualLines[i], false, $"Line {i}.");
                }
                if (expectedLines.Length + 1 == actualLines.Length) {
                    string lastLine = actualLines.Last();
                    Assert.IsTrue(string.IsNullOrWhiteSpace(lastLine), lastLine);
                    continue;
                }
                Assert.AreEqual(expectedLines.Length, actualLines.Length, "File length");
            }
        }
    }
}