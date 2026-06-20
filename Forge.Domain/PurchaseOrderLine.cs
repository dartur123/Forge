namespace Forge.Domain;

public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCostForeign { get; set; }
    public decimal UnitCostPhp { get; set; }
}