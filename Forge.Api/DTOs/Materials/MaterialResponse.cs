using Forge.Domain;

namespace Forge.Api.DTOs.Materials
{
    public class MaterialResponse
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public static MaterialResponse FromEntity(Material material) => new()
        {
            Id = material.Id,
            Sku = material.Sku,
            Name = material.Name,
            Type = material.Type.ToString(),
            UnitOfMeasure = material.UnitOfMeasure,
            Description = material.Description,
            IsActive = material.IsActive
        };
    }
}