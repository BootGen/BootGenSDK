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
        public IDisk Disk { get; set; }
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
                {"project_title", projectName},
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
            typeScriptGenerator.RenderClasses($"{ClientFolder}/models", s => $"{s.Name}.{ClientExtension}", "client/model.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/views", s => $"{s.Name}List.{ClientComponentExtension}", "client/model_list.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/components", s => $"{s.Name}View.{ClientComponentExtension}", "client/model_view.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderClasses($"{ClientFolder}/components", s => $"{s.Name}Edit.{ClientComponentExtension}", "client/model_edit.sbn", DataModel.CommonClasses);
            typeScriptGenerator.RenderResources($"{ClientFolder}/store", s => $"{s.Name}Module.{ClientExtension}", "client/store_module.sbn", ResourceCollection.RootResources);
            typeScriptGenerator.Render($"{ClientFolder}/router", $"index.{ClientExtension}", "client/router.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            typeScriptGenerator.Render($"{ClientFolder}", $"App.{ClientComponentExtension}", "client/app.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            typeScriptGenerator.Render($"{ClientFolder}/api", $"index.{ClientExtension}", "client/api_client.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses}
            });
            typeScriptGenerator.Render($"{ClientFolder}/store", $"index.{ClientExtension}", "client/store.sbn", new Dictionary<string, object> {
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
