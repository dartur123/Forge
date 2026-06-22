namespace Forge.Api.DTOs.Location;

public class LocationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationTypeResponse Type { get; set; } = null!;
    public LocationResponse? ParentLocation { get; set; }

    public static LocationResponse FromEntity(Forge.Domain.Location location) => new()
    {
        Id = location.Id,
        Name = location.Name,
        Type = LocationTypeResponse.FromEntity(location.LocationType),
        ParentLocation = location.ParentLocation is null
            ? null
            : FromEntity(location.ParentLocation)
    };
}