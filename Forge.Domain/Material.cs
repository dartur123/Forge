using Forge.Domain.Enums;

namespace Forge.Domain;

public class Material
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Lot> Lots { get; set; } = new();
}