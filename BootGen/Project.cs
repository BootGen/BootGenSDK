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
        public IDisk Disk { get; set; }
        public Api Api { get; set; }
        private DataModel DataModel => ResourceCollection.DataModel;
        public ResourceCollection ResourceCollection { get; set; }
        public SeedDataStore SeedStore { get; set; }
        public string TemplateRoot { get; set; }
        
        public void GenerateFiles(string projectName, string namespce, string baseUrl)
        {
            IDisk disk = Disk;
            GenerateFiles(projectName, namespce, baseUrl, disk);
        }

        private void GenerateFiles(string projectName, string namespce, string baseUrl, IDisk disk)
        {
            var aspNetCoreGenerator = new AspNetCoreGenerator(disk);
            aspNetCoreGenerator.NameSpace = namespce;
            aspNetCoreGenerator.TemplateRoot = TemplateRoot;
            var pivotResources = Api.NestedResources.Where(r => r.Pivot != null && r.GenerationSettings.GenerateController).ToList();
            var pivotClasses = Api.NestedResources.Where(r => r.Pivot != null && r.GenerationSettings.GenerateService).Select(r => r.Pivot).Distinct().ToList();
            var oasGenerator = new OASGenerator(disk);
            oasGenerator.TemplateRoot = TemplateRoot;
            oasGenerator.RenderApi("", "restapi.yml", "oas3template.sbn", projectName, Api);
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "server/resourceController.sbn", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateController).ToList());
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "server/nestedResourceController.sbn", Api.NestedResources.Where(r => r.Pivot == null && r.GenerationSettings.GenerateController).ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"I{AspNetCoreGenerator.ServiceName(r)}.cs", "server/resourceServiceInterface.sbn", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateServiceInterface).ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"{AspNetCoreGenerator.ServiceName(r)}.cs", "server/resourceService.sbn", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateService).ToList());

            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "server/pivotController.sbn", pivotResources);
            aspNetCoreGenerator.RenderClasses(ServiceFolder, c => $"I{c.Name.Plural}Service.cs", "server/pivotServiceInterface.sbn", pivotClasses);
            aspNetCoreGenerator.RenderClasses(ServiceFolder, c => $"{c.Name.Plural}Service.cs", "server/pivotService.sbn", pivotClasses);

            aspNetCoreGenerator.RenderClasses("", s => $"{s.Name}.cs", "server/csharp_model.sbn", DataModel.Classes);
            aspNetCoreGenerator.RenderEnums("", s => $"{s.Name}.cs", "server/csharp_enum.sbn", DataModel.Enums);
            aspNetCoreGenerator.Render("", "DataContext.cs", "server/dataContext.sbn", new Dictionary<string, object> {
                {"classes", DataModel.StoredClasses},
                {"seedList", SeedStore.All()},
                {"name_space", namespce}
            });
            aspNetCoreGenerator.Render("", "ServiceRegistrator.cs", "server/resourceRegistration.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources.Where(r => r.GenerationSettings.GenerateService && r.GenerationSettings.GenerateServiceInterface)},
                {"pivots", pivotClasses}
            });
            var typeScriptGenerator = new TypeScriptGenerator(disk);
            typeScriptGenerator.TemplateRoot = TemplateRoot;
            typeScriptGenerator.RenderClasses($"{ClientFolder}/models", s => $"{s.Name}.ts", "client/ts_model.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderEnums($"{ClientFolder}/models", s => $"{s.Name}.ts", "client/ts_enum.sbn", DataModel.Enums);
            typeScriptGenerator.RenderResources($"{ClientFolder}/store", s => $"{s.Name}Module.ts", "client/vuex_module.sbn", Api.RootResources);
            typeScriptGenerator.Render($"{ClientFolder}/store", "index.ts", "client/vuex.sbn", new Dictionary<string, object> {
                {"classes", Api.RootResources.Select(r => r.Class)},
                {"base_url", baseUrl}
            });
        }

        public void DeleteGeneratedFiles()
        {
            var vdisk = new VirtualDisk();
            GenerateFiles(string.Empty, string.Empty, string.Empty, vdisk);
            foreach(var file in vdisk.Files)
            {
                var path = file.Name;
                if (!string.IsNullOrWhiteSpace(file.Path))
                    path = System.IO.Path.Combine(file.Path, file.Name);
                Disk.Delete(path);
            }
        }
    }

}
