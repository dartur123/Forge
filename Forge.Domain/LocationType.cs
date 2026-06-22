namespace Forge.Domain;

public class LocationType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Location> Locations { get; set; } = new();
    
}