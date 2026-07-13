using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class Location
{
    protected Location() { }
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int? ParentLocationId { get; private set; }
    public Location? ParentLocation { get; private set; }
    public int LocationTypeId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public LocationType LocationType { get; private set; } = null!;

    public static Location Create(string name, int? parentLocationId, int locationTypeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        return new Location
        {
            Name = name,
            ParentLocationId = parentLocationId,
            LocationTypeId = locationTypeId
        };
    }

    public void Update(string name, int? parentLocationId, int locationTypeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        Name = name;
        ParentLocationId = parentLocationId;
        LocationTypeId = locationTypeId;
    }

    public void Deactivate() => IsActive = false;
}