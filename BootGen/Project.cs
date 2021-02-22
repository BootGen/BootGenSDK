using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BootGen;

namespace BootGen
{
    public class Project
    {
        public string ControllerFolder { get; set; }
        public string ServiceFolder { get; set; }
        public string ClientFolder { get; set; }
        public string ProjectFolder { get; set; }
        public Api Api { get; set; }
        private DataModel DataModel => ResourceCollection.DataModel;
        public ResourceCollection ResourceCollection { get; set; }
        public SeedDataStore SeedStore { get; set; }
        
        public void GenerateFiles(string projectName, string namespce, string baseUrl)
        {
            var disk = new Disk(ProjectFolder);
            var aspNetCoreGenerator = new AspNetCoreGenerator(disk);
            aspNetCoreGenerator.NameSpace = namespce;
            var pivotResources = Api.NestedResources.Where(r => r.Pivot != null && r.GenerationSettings.GenerateController).ToList();
            var pivotClasses = Api.NestedResources.Where(r => r.Pivot != null && r.GenerationSettings.GenerateService).Select(r => r.Pivot).Distinct().ToList();
            new OASGenerator(disk).RenderApi("", "restapi.yml", "templates/oas3template.sbn", projectName, Api);
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "templates/server/resourceController.sbn", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateController).ToList());
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "templates/server/nestedResourceController.sbn", Api.NestedResources.Where(r => r.Pivot == null && r.GenerationSettings.GenerateController).ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"I{AspNetCoreGenerator.ServiceName(r)}.cs", "templates/server/resourceServiceInterface.sbn", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateServiceInterface).ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"{AspNetCoreGenerator.ServiceName(r)}.cs", "templates/server/resourceService.sbn", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateService).ToList());

            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "templates/server/pivotController.sbn", pivotResources);
            aspNetCoreGenerator.RenderClasses(ServiceFolder, c => $"I{c.Name.Plural}Service.cs", "templates/server/pivotServiceInterface.sbn", pivotClasses);
            aspNetCoreGenerator.RenderClasses(ServiceFolder, c => $"{c.Name.Plural}Service.cs", "templates/server/pivotService.sbn", pivotClasses);

            aspNetCoreGenerator.RenderClasses("", s => $"{s.Name}.cs", "templates/server/csharp_model.sbn", DataModel.Classes);
            aspNetCoreGenerator.RenderEnums("", s => $"{s.Name}.cs", "templates/server/csharp_enum.sbn", DataModel.Enums);
            aspNetCoreGenerator.Render("", "DataContext.cs", "templates/server/dataContext.sbn", new Dictionary<string, object> {
                {"classes", DataModel.StoredClasses},
                {"seedList", SeedStore.All()},
                {"name_space", namespce}
            });
            aspNetCoreGenerator.Render("", "ServiceRegistrator.cs", "templates/server/resourceRegistration.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateService && r.GenerationSettings.GenerateServiceInterface)},
                {"pivots", pivotClasses}
            });
            var typeScriptGenerator = new TypeScriptGenerator(disk);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/models", s => $"{s.Name}.ts", "templates/client/ts_model.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderEnums($"{ClientFolder}/models", s => $"{s.Name}.ts", "templates/client/ts_enum.sbn", DataModel.Enums);
            typeScriptGenerator.RenderResources($"{ClientFolder}/store", s => $"{s.Name}Module.ts", "templates/client/vuex_module.sbn", Api.RootResources);
            typeScriptGenerator.Render($"{ClientFolder}/store", "index.ts", "templates/client/vuex.sbn", new Dictionary<string, object> {
                {"classes", Api.RootResources.Select(r => r.Class)},
                {"base_url", baseUrl}
            });
        }

        public void DeleteGeneratedFiles()
        {
            var pivotClasses = Api.NestedResources.Where(r => r.Pivot != null && r.GenerationSettings.GenerateService).Select(r => r.Pivot).Distinct().ToList();
            Delete("restapi.yml");
            foreach (var resource in ResourceCollection.RootResources)
            {
                if (resource.GenerationSettings.GenerateController)
                    Delete(ControllerFolder, $"{AspNetCoreGenerator.ControllerName(resource)}.cs");
                if (resource.GenerationSettings.GenerateServiceInterface)
                    Delete(ServiceFolder, $"I{AspNetCoreGenerator.ServiceName(resource)}.cs");
                if (resource.GenerationSettings.GenerateService)
                    Delete(ServiceFolder, $"{AspNetCoreGenerator.ServiceName(resource)}.cs");
                Delete(ClientFolder, "store", $"{resource.Name}Module.ts");
            }
            foreach (var resource in Api.NestedResources)
            {
                if (resource.GenerationSettings.GenerateController)
                    Delete(ControllerFolder, $"{AspNetCoreGenerator.ControllerName(resource)}.cs");
            }
            foreach (var c in pivotClasses)
            {
                Delete(ServiceFolder, $"I{c.Name.Plural}Service.cs");
                Delete(ServiceFolder, $"{c.Name.Plural}Service.cs");
            }
            foreach (var c in DataModel.Classes)
            {
                Delete($"{c.Name}.cs");
                Delete(ClientFolder, "models", $"{c.Name}.ts");
            }
            foreach (var e in DataModel.Enums)
            {
                Delete($"{e.Name}.cs");
                Delete(ClientFolder, "models", $"{e.Name}.ts");
            }
            Delete(ClientFolder, "store", "index.ts");
            Delete("ServiceRegistrator.cs");
            Delete("DataContext.cs");
        }
        private void Delete(string path1, string path2, string path3) {
            string path = System.IO.Path.Combine(ProjectFolder, path1, path2, path3);
            Console.WriteLine(path);
            File.Delete(path);
        }
        private void Delete(string path1, string path2) {
            string path = System.IO.Path.Combine(ProjectFolder, path1, path2);
            Console.WriteLine(path);
            File.Delete(path);
        }
        private void Delete(string path1) {
            string path = System.IO.Path.Combine(ProjectFolder, path1);
            Console.WriteLine(path);
            File.Delete(path);
        }
    }

}
