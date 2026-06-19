using System.ComponentModel.DataAnnotations;

namespace Forge.Domain;

public enum MaterialType
{
    Raw,
    WorkInProgress,
    FinishedGood
}

public class Material
{
    public int Id { get; set; }
    [Required]
    [MinLength(1)]
    public string Sku { get; set; } = string.Empty;
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    [Required]
    [MinLength(1)]
    public string UnitOfMeasure { get; set; } = string.Empty;
}