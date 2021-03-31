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
        public string EntityFolder { get; set; }
        public string ClientFolder { get; set; }
        public string ClientExtension { get; set; }
        public string ClientComponentExtension { get; set; }
        public IDisk Disk { get; set; }
        public Api Api { get; set; }
        private DataModel DataModel => ResourceCollection.DataModel;
        public ResourceCollection ResourceCollection { get; set; }
        public SeedDataStore SeedStore { get; set; }
        public string TemplateRoot { get; set; }
        
        public void GenerateFiles(string projectName, string namespce, string baseUrl)
        {
            GenerateFiles(projectName, namespce, baseUrl, Disk);
        }

        private void GenerateFiles(string projectName, string namespce, string baseUrl, IDisk disk)
        {
            Api.BaseUrl = baseUrl;
            var aspNetCoreGenerator = new AspNetCoreGenerator(disk);
            aspNetCoreGenerator.NameSpace = namespce;
            aspNetCoreGenerator.TemplateRoot = TemplateRoot;
            var pivotResources = Api.NestedResources.Where(r => r.Pivot != null).ToList();
            var pivotClasses = pivotResources.Select(r => r.Pivot).Distinct().ToList();
            var oasGenerator = new OASGenerator(disk);
            oasGenerator.TemplateRoot = TemplateRoot;
            oasGenerator.RenderApi("", "restapi.yml", "oas3template.sbn", projectName, Api);
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "server/resourceController.sbn", ResourceCollection.RootResources.ToList());
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "server/nestedResourceController.sbn", Api.NestedResources.Where(r => r.Pivot == null).ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"I{AspNetCoreGenerator.ServiceName(r)}.cs", "server/resourceServiceInterface.sbn", ResourceCollection.RootResources.ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"{AspNetCoreGenerator.ServiceName(r)}.cs", "server/resourceService.sbn", ResourceCollection.RootResources.ToList());

            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{AspNetCoreGenerator.ControllerName(r)}.cs", "server/pivotController.sbn", pivotResources);
            aspNetCoreGenerator.RenderClasses(ServiceFolder, c => $"I{c.Name.Plural}Service.cs", "server/pivotServiceInterface.sbn", pivotClasses);
            aspNetCoreGenerator.RenderClasses(ServiceFolder, c => $"{c.Name.Plural}Service.cs", "server/pivotService.sbn", pivotClasses);

            aspNetCoreGenerator.RenderClasses(EntityFolder, s => $"{s.Name}.cs", "server/entity.sbn", DataModel.Classes);
            aspNetCoreGenerator.Render("", "ApplicationDbContext.cs", "server/applicationDbContext.sbn", new Dictionary<string, object> {
                {"classes", DataModel.Classes},
                {"seedList", SeedStore.All()},
                {"name_space", namespce}
            });
            aspNetCoreGenerator.Render("", "ServiceRegistrator.cs", "server/resourceRegistration.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"pivots", pivotClasses}
            });
            var typeScriptGenerator = new TypeScriptGenerator(disk);
            typeScriptGenerator.TemplateRoot = TemplateRoot;
            typeScriptGenerator.RenderClasses($"{ClientFolder}/models", s => $"{s.Name}.{ClientExtension}", "client/model.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/views", s => $"{s.Name}List.{ClientComponentExtension}", "client/model_list.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/components", s => $"{s.Name}View.{ClientComponentExtension}", "client/model_view.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/components", s => $"{s.Name}Edit.{ClientComponentExtension}", "client/model_edit.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderResources($"{ClientFolder}/store", s => $"{s.Name}Module.{ClientExtension}", "client/store_module.sbn", Api.RootResources);
            typeScriptGenerator.Render($"{ClientFolder}/router", $"index.{ClientExtension}", "client/router.sbn", new Dictionary<string, object> {
                {"classes", Api.RootResources.Select(r => r.Class)}
            });
            typeScriptGenerator.Render($"{ClientFolder}", $"App.{ClientComponentExtension}", "client/app.sbn", new Dictionary<string, object> {
                {"classes", Api.RootResources.Select(r => r.Class)}
            });
            typeScriptGenerator.Render($"{ClientFolder}/store", $"index.{ClientExtension}", "client/store.sbn", new Dictionary<string, object> {
                {"classes", Api.RootResources.Select(r => r.Class)},
                {"base_url", baseUrl}
            });
        }
    }

}
