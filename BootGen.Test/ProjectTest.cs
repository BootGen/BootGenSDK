using Microsoft.VisualStudio.TestTools.UnitTesting;
using BootGen;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BootGenTest
{
    [TestClass]
    public class ProjectTest
    {

        [TestMethod]
        public void TestGenerate()
        {
            TestWithTemplates("templates", "example_input.json", "SampleOutput", new Dictionary<string, ClassSettings> {{ "Task", new ClassSettings {
                HasTimestamps = true,
                PropertySettings = new Dictionary<string, PropertySettings> {
                    {"Tags",
                    new PropertySettings {
                        IsManyToMany = true
                    }
                    }
                }
            }}});
        }

        [TestMethod]
        public void TestGenerateKebab()
        {
            TestWithTemplates("templates", "example_input_kebab_case.json", "SampleOutputKebab");
        }

        [TestMethod]
        public void TestWithoutTemplates()
        {
            GenerateWithTemplates("does_not_exists", "example_input.json");
        }

        private static void TestWithTemplates(string templateRoot, string fileName, string outputFolder, Dictionary<string, ClassSettings> classSettings = null)
        {
            VirtualDisk disk = GenerateWithTemplates(templateRoot, fileName, classSettings);
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

            ZipFile.ExtractToDirectory($"{outputFolder}.zip", ".");
            foreach (var file in disk.Files)
            {
                var path = System.IO.Path.Combine(outputFolder, file.Path, file.Name);
                var expectedLines = File.ReadAllLines(path);
                var actualLines = file.Content.Split(Environment.NewLine);
                for (int i = 0; i < expectedLines.Length; ++i)
                {
                    Assert.AreEqual(expectedLines[i].Trim(), actualLines[i].Trim(), false, $"{file.Name} line {i}.");
                }
                if (expectedLines.Length + 1 == actualLines.Length)
                {
                    string lastLine = actualLines.Last();
                    Assert.IsTrue(string.IsNullOrWhiteSpace(lastLine), lastLine);
                    continue;
                }
                Assert.AreEqual(expectedLines.Length, actualLines.Length, "File length");
            }
        }

        private static VirtualDisk GenerateWithTemplates(string templateRoot, string fileName, Dictionary<string, ClassSettings> classSettings = null)
        {
            var data = JObject.Parse(File.ReadAllText(fileName));
            var dataModel = new DataModel();
            dataModel.Load(data, classSettings);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
            var disk = new VirtualDisk();
            var project = new ServerProject
            {
                Config = new ServerConfig
                {
                    ControllerFolder = "Controllers",
                    ServiceFolder = "Services",
                    EntityFolder = "Models"
                },
                Disk = disk,
                ResourceCollection = resourceCollection,
                SeedStore = seedStore,
                Templates = new Disk(Path.Combine(templateRoot, "server"))
            };
            project.GenerateFiles("TestProject", "http://localhost:5000");

            var clientDisk = new VirtualDisk();
            var clientProject = new ClientProject
            {
                Config = new ClientConfig
                {
                    Extension = "ts",
                    ComponentExtension = "vue",
                    RouterFileName = "index.ts",
                },
                Disk = clientDisk,
                ResourceCollection = resourceCollection,
                SeedStore = seedStore,
                Templates = new Disk(Path.Combine(templateRoot, "client"))
            };
            clientProject.GenerateFiles("TestProject", "http://localhost:5000");
            disk.Mount(clientDisk, "ClientApp/src");
            return disk;
        }
    }
}