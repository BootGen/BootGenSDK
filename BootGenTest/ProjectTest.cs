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
            var data = JObject.Parse(File.ReadAllText("example_input.json"), new JsonLoadSettings { CommentHandling = CommentHandling.Load });
            var dataModel = new DataModel();
            dataModel.Load(data);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
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
            try {
                ZipFile.ExtractToDirectory("SampleOutput.zip", ".");
            } catch {
                if (File.Exists("SampleOutput.zip")) {
                    Assert.Fail($"SampleOutput.zip length: {new FileInfo("SampleOutput.zip").Length}");
                } else {
                    Assert.Fail("SampleOutput.zip is missing!");
                }
            }
            foreach (var file in disk.Files) {
               var path = System.IO.Path.Combine("SampleOutput", file.Path, file.Name);
                var expectedLines = File.ReadAllLines(path);
                var actualLines = file.Content.Split(Environment.NewLine);
                for (int i = 0; i < expectedLines.Length; ++i) {
                    Assert.AreEqual(expectedLines[i], actualLines[i], false, $"{file.Name} line {i}.");
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