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
            TestWithTemplates("templates", "example_input.json", "SampleOutput");
        }

        [TestMethod]
        public void TestGenerateKebab()
        {
            TestWithTemplates("templates", "example_input_kebab_case.json", "SampleOutputKebab");
        }


        [TestMethod]
        public void TestGenerateWrongAnnotation()
        {
            try
            {
                GenerateWithTemplates("templates", "example_input_wrong_annotation.json");
                Assert.Fail();
            } catch (Exception e) {
                Assert.IsTrue(e.Message.StartsWith("Unrecognised annotation:"));
            }
        }

        [TestMethod]
        public void TestGenerateInvalidClassName()
        {
            try
            {
                GenerateWithTemplates("templates", "example_input_invalid_class_name.json");
                Assert.Fail();
            } catch (Exception e) {
                Assert.IsTrue(e.Message.StartsWith("Invalid class name:"));
            }
        }

        [TestMethod]
        public void TestWithoutTemplates()
        {
            GenerateWithTemplates("does_not_exists", "example_input.json");
        }

        private static void TestWithTemplates(string templateRoot, string fileName, string outputFolder)
        {
            VirtualDisk disk = GenerateWithTemplates(templateRoot, fileName);
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
                    Assert.AreEqual(expectedLines[i], actualLines[i], false, $"{file.Name} line {i}.");
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

        private static VirtualDisk GenerateWithTemplates(string templateRoot, string fileName)
        {
            var data = JObject.Parse(File.ReadAllText(fileName), new JsonLoadSettings { CommentHandling = CommentHandling.Load });
            var dataModel = new DataModel();
            dataModel.Load(data);
            var resourceCollection = new ResourceCollection(dataModel);
            var seedStore = new SeedDataStore(resourceCollection);
            seedStore.Load(data);
            var disk = new VirtualDisk();
            var project = new ServerProject
            {
                ControllerFolder = "Controllers",
                ServiceFolder = "Services",
                EntityFolder = "Models",
                Disk = disk,
                ResourceCollection = resourceCollection,
                SeedStore = seedStore,
                Templates = new Disk(Path.Combine(templateRoot, "server"))
            };
            project.GenerateFiles("TestProject", "http://localhost:5000");
            var clientProject = new ClientProject
            {
                Folder = "ClientApp/src",
                Extension = "ts",
                ComponentExtension = "vue",
                RouterFileName = "index.ts",
                Disk = disk,
                ResourceCollection = resourceCollection,
                SeedStore = seedStore,
                Templates = new Disk(Path.Combine(templateRoot, "client"))
            };
            clientProject.GenerateFiles("TestProject", "http://localhost:5000");
            return disk;
        }
    }
}