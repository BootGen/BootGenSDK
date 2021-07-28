using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BootGen;

namespace BootGen
{
    public class ServerProject
    {
        public ServerConfig Config { get; set; }
        public IDisk Disk { get; set; }
        private DataModel DataModel => ResourceCollection.DataModel;
        public ResourceCollection ResourceCollection { get; set; }
        public SeedDataStore SeedStore { get; set; }
        public IDisk Templates { get; set; }
        
        public void GenerateFiles(string namespce, string baseUrl)
        {
            GenerateFiles(namespce, baseUrl, Disk);
        }

        private void GenerateFiles(string namespce, string baseUrl, IDisk disk)
        {
            var aspNetCoreGenerator = new AspNetCoreGenerator(disk);
            aspNetCoreGenerator.NameSpace = namespce;
            aspNetCoreGenerator.Templates = Templates;
            var pivotResources = ResourceCollection.NestedResources.Where(r => r.Pivot != null).ToList();
            var pivotClasses = pivotResources.Select(r => r.Pivot).Distinct().ToList();
            var oasGenerator = new OASGenerator(disk);
            oasGenerator.Templates = Templates;
            oasGenerator.Render("", "restapi.yml", "oas3template.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses},
                {"project_title", namespce},
                {"base_url", baseUrl}
            });
            aspNetCoreGenerator.RenderResources(Config.ControllerFolder, r => $"{FullName(r)}Controller.cs", "resourceController.sbn", ResourceCollection.RootResources.ToList());
            aspNetCoreGenerator.RenderResources(Config.ControllerFolder, r => $"{FullName(r)}Controller.cs", "nestedResourceController.sbn", ResourceCollection.NestedResources.Where(r => r.Pivot == null).ToList());
            aspNetCoreGenerator.RenderResources($"{Config.ServiceFolder}/Interfaces", r => $"I{FullName(r)}Service.cs", "resourceServiceInterface.sbn", ResourceCollection.RootResources.ToList());
            aspNetCoreGenerator.RenderResources(Config.ServiceFolder, r => $"{FullName(r)}Service.cs", "resourceService.sbn", ResourceCollection.RootResources.ToList());

            aspNetCoreGenerator.RenderResources(Config.ControllerFolder, r => $"{FullName(r)}Controller.cs", "pivotController.sbn", pivotResources);

            aspNetCoreGenerator.RenderClasses(Config.EntityFolder, s => $"{s.Name}.cs", "entity.sbn", DataModel.Classes.Where(c => !c.IsPivot));
            aspNetCoreGenerator.Render("", "ApplicationDbContext.cs", "applicationDbContext.sbn", new Dictionary<string, object> {
                {"classes", DataModel.Classes},
                {"seedList", SeedStore.All()},
                {"name_space", namespce}
            });
            aspNetCoreGenerator.Render("", "Startup.cs", "startup.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources}
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
