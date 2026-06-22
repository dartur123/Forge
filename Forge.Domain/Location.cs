namespace Forge.Domain;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentLocationId { get; set; }
    public Location? ParentLocation { get; set; }
    public int LocationTypeId { get; set; }
    public LocationType LocationType { get; set; } = null!;
}