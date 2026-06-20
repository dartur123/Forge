using Forge.Domain.Enums;

namespace Forge.Domain;

public class BillOfMaterialsLine
{
    public int Id { get; set; }
    public int BillOfMaterialsId { get; set; }
    public int? ParentLineId { get; set; }
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public BomLineSourceType SourceType { get; set; }

    public List<BillOfMaterialsLine> Children { get; set; } = new();
}