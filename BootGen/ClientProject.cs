using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BootGen;

namespace BootGen
{
    public class ClientProject
    {
        public string Folder { get; set; }
        public string Extension { get; set; }
        public string RouterExtension { get; set; }
        public string ComponentExtension { get; set; }
        public string ModelsFolder { get; set; } = "models";
        public string ViewsFolder { get; set; } = "views";
        public string ComponentsFolder { get; set; } = "components";
        public string StoreFolder { get; set; } = "store";
        public string RouterFolder { get; set; } = "router";
        public string ApiFolder { get; set; } = "api";
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
            var pivotResources = ResourceCollection.NestedResources.Where(r => r.Pivot != null).ToList();
            var pivotClasses = pivotResources.Select(r => r.Pivot).Distinct().ToList();
            var generator = new TypeScriptGenerator(disk);
            generator.TemplateRoot = TemplateRoot;
            generator.RenderClasses($"{Folder}/{ModelsFolder}", s => $"{s.Name}.{Extension}", "model.sbn", DataModel.CommonClasses);
            generator.RenderClasses($"{Folder}/{ViewsFolder}", s => $"{s.Name}List.{ComponentExtension}", "model_list.sbn", DataModel.CommonClasses);
            generator.RenderClasses($"{Folder}/{ComponentsFolder}", s => $"{s.Name}View.{ComponentExtension}", "model_view.sbn", DataModel.CommonClasses);
            generator.RenderClasses($"{Folder}/{ComponentsFolder}", s => $"{s.Name}Edit.{ComponentExtension}", "model_edit.sbn", DataModel.CommonClasses);
            generator.RenderResources($"{Folder}/{StoreFolder}", s => $"{s.Name}Module.{Extension}", "store_module.sbn", ResourceCollection.RootResources);
            generator.Render($"{Folder}/{RouterFolder}", $"index.{RouterExtension}", "router.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            generator.Render(Folder, $"App.{ComponentExtension}", "app.sbn", new Dictionary<string, object> {
                {"classes", DataModel.CommonClasses}
            });
            generator.Render($"{Folder}/{ApiFolder}", $"index.{Extension}", "api_client.sbn", new Dictionary<string, object> {
                {"resources", ResourceCollection.RootResources},
                {"classes", DataModel.CommonClasses}
            });
            generator.Render($"{Folder}/{StoreFolder}", $"index.{Extension}", "store.sbn", new Dictionary<string, object> {
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
