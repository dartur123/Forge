using Forge.Domain;

namespace Forge.Api.DTOs.Location
{
    public class LocationTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public static LocationTypeResponse FromEntity(LocationType location) => new()
        {
            Id = location.Id,
            Name = location.Name,
            IsActive = location.IsActive
        };
    }
}