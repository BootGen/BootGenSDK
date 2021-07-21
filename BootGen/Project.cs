using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public string ClientModelsFolder { get; set; } = "models";
        public string ClientViewsFolder { get; set; } = "views";
        public string ClientComponentsFolder { get; set; } = "components";
        public string ClientStoreFolder { get; set; } = "store";
        public string ClientRouterFolder { get; set; } = "router";
        public string ClientApiFolder { get; set; } = "api";
        public string ClientRouterExtension { get; set; } = "js";
        public IDisk Disk { get; set; }
        private DataModel DataModel => ResourceCollection.DataModel;
        public ResourceCollection ResourceCollection { get; set; }
        public SeedDataStore SeedStore { get; set; }
        public string TemplateRoot { get; set; }
        
        public void GenerateFiles(string namespce, string baseUrl)
        {
            GenerateFiles(namespce, baseUrl, Disk);
        }

        private void GenerateFiles(string namespce, string baseUrl, IDisk disk)
        {
            var aspNetCoreGenerator = new AspNetCoreGenerator(disk);
            aspNetCoreGenerator.NameSpace = namespce;
            aspNetCoreGenerator.TemplateRoot = TemplateRoot;
            var pivotResources = ResourceCollection.NestedResources.Where(r => r.Pivot != null).ToList();
            var pivotClasses = pivotResources.Select(r => r.Pivot).Distinct().ToList();
            var oasGenerator = new OASGenerator(disk);
            oasGenerator.TemplateRoot = TemplateRoot;
            oasGenerator.Render("", "restapi.yml", "oas3template.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses},
                {"project_title", namespce},
                {"base_url", baseUrl}
            });
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{FullName(r)}Controller.cs", "server/resourceController.sbn", ResourceCollection.RootResources.ToList());
            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{FullName(r)}Controller.cs", "server/nestedResourceController.sbn", ResourceCollection.NestedResources.Where(r => r.Pivot == null).ToList());
            aspNetCoreGenerator.RenderResources($"{ServiceFolder}/Interfaces", r => $"I{FullName(r)}Service.cs", "server/resourceServiceInterface.sbn", ResourceCollection.RootResources.ToList());
            aspNetCoreGenerator.RenderResources(ServiceFolder, r => $"{FullName(r)}Service.cs", "server/resourceService.sbn", ResourceCollection.RootResources.ToList());

            aspNetCoreGenerator.RenderResources(ControllerFolder, r => $"{FullName(r)}Controller.cs", "server/pivotController.sbn", pivotResources);

            aspNetCoreGenerator.RenderClasses(EntityFolder, s => $"{s.Name}.cs", "server/entity.sbn", DataModel.Classes.Where(c => !c.IsPivot));
            aspNetCoreGenerator.Render("", "ApplicationDbContext.cs", "server/applicationDbContext.sbn", new Dictionary<string, object> {
                {"classes", DataModel.Classes},
                {"seedList", SeedStore.All()},
                {"name_space", namespce}
            });
            aspNetCoreGenerator.Render("", "Startup.cs", "server/startup.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources}
            });
            var typeScriptGenerator = new TypeScriptGenerator(disk);
            typeScriptGenerator.TemplateRoot = TemplateRoot;
            typeScriptGenerator.RenderClasses($"{ClientFolder}/{ClientModelsFolder}", s => $"{s.Name}.{ClientExtension}", "client/model.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/{ClientViewsFolder}", s => $"{s.Name}List.{ClientComponentExtension}", "client/model_list.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/{ClientComponentsFolder}", s => $"{s.Name}View.{ClientComponentExtension}", "client/model_view.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/{ClientComponentsFolder}", s => $"{s.Name}Edit.{ClientComponentExtension}", "client/model_edit.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderResources($"{ClientFolder}/{ClientStoreFolder}", s => $"{s.Name}Module.{ClientExtension}", "client/store_module.sbn", ResourceCollection.RootResources);
            typeScriptGenerator.Render($"{ClientFolder}/{ClientRouterFolder}", $"index.{ClientRouterExtension}", "client/router.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            typeScriptGenerator.Render(ClientFolder, $"App.{ClientComponentExtension}", "client/app.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            typeScriptGenerator.Render($"{ClientFolder}/{ClientApiFolder}", $"index.{ClientExtension}", "client/api_client.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses}
            });
            typeScriptGenerator.Render($"{ClientFolder}/{ClientStoreFolder}", $"index.{ClientExtension}", "client/store.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses},
                {"base_url", baseUrl}
            });
        }

        private static string FullName(Resource resource)
        {
            var builder = new StringBuilder();
            if (resource is NestedResource nestedResource && nestedResource.ParentResource != null)
                builder.Append(nestedResource.ParentResource.Name.Plural);
            builder.Append(resource.Name.Plural);
            return builder.ToString();
        }
    }

}
