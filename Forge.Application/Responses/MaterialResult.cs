using Forge.Domain;

namespace Forge.Application.Responses;

public class MaterialResult
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public static MaterialResult FromEntity(Material material) => new()
    {
        Id = material.Id,
        Sku = material.Sku,
        Name = material.Name,
        Type = material.Type.ToString(),
        Description = material.Description,
        UnitOfMeasure = material.UnitOfMeasure,
        IsActive = material.IsActive
    };
}