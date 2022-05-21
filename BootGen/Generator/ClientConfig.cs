namespace BootGen;

public class ClientConfig {

    public string Extension { get; set; }
    public string RouterFileName { get; set; }
    public string ComponentExtension { get; set; }
    public string ModelsFolder { get; set; } = "models";
    public string ViewsFolder { get; set; } = "views";
    public string ComponentsFolder { get; set; } = "components";
    public string StoreFolder { get; set; } = "store";
    public string RouterFolder { get; set; } = "router";
    public string ApiFolder { get; set; } = "api";
}
