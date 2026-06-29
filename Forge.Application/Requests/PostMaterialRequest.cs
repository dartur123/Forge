using Forge.Domain.Enums;

namespace Forge.Application.Requests
{
    public class PostMaterialRequest
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public MaterialType Type { get; set; }
        public string? Description { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}
