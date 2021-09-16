using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootGen
{
    public class ClientProject
    {
        public ClientConfig Config { get; set; } 
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
            var pivotResources = ResourceCollection.NestedResources.Where(r => r.Pivot != null).ToList();
            var pivotClasses = pivotResources.Select(r => r.Pivot).Distinct().ToList();
            var generator = new TypeScriptGenerator(disk);
            generator.Templates = Templates;
            generator.RenderClasses($"{Config.ModelsFolder}", s => $"{s.Name}.{Config.Extension}", "model.sbn", DataModel.CommonClasses);
            generator.RenderClasses($"{Config.ViewsFolder}", s => $"{s.Name}List.{Config.ComponentExtension}", "model_list.sbn", DataModel.CommonClasses);
            generator.RenderClasses($"{Config.ComponentsFolder}", s => $"{s.Name}View.{Config.ComponentExtension}", "model_view.sbn", DataModel.CommonClasses);
            generator.RenderClasses($"{Config.ComponentsFolder}", s => $"{s.Name}Edit.{Config.ComponentExtension}", "model_edit.sbn", DataModel.CommonClasses);
            generator.RenderResources($"{Config.StoreFolder}", s => $"{s.Name}Module.{Config.Extension}", "store_module.sbn", ResourceCollection.RootResources);
            generator.Render($"{Config.RouterFolder}", Config.RouterFileName, "router.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            generator.Render("", $"App.{Config.ComponentExtension}", "app.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses}
            });
            generator.Render($"{Config.ApiFolder}", $"index.{Config.Extension}", "api_client.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses},
                {"base_url", baseUrl}
            });
            generator.Render($"{Config.StoreFolder}", $"index.{Config.Extension}", "store.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
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
