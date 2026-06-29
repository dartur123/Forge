using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class LocationType
{
    protected LocationType() { }
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public List<Location> Locations { get; private set; } = new();

    public static LocationType Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        return new LocationType
        {
            Name = name
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");
        Name = name;
    }

    public void Deactivate() => IsActive = false;
}