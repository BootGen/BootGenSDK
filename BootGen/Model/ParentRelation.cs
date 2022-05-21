namespace BootGen;
internal class ParentRelation
{
    public ParentRelation(Resource resource, string name = null)
    {
        Resource = resource;
        Name = name ?? resource.Name;
    }
    public string Name { get; set; }
    public Resource Resource { get; set; }
}