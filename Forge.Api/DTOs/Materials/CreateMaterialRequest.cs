using Forge.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Forge.Api.DTOs.Materials
{
    public class CreateMaterialRequest
    {
        [Required]
        [MaxLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public MaterialType Type { get; set; }

        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}
