namespace Forge.Api.DTOs.Location
{
    public class LocationTreeNode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public LocationTypeResponse Type { get; set; } = null!;
        public List<LocationTreeNode> Children { get; set; } = new();
    }
}
