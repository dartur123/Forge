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
    }
}
