using Forge.Domain;

namespace Forge.Application.Responses;

public class PurchaseOrderLineResult
{
    public int Id { get; private set; }
    public int PurchaseOrderId { get; private set; }
    public int MaterialId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCostForeign { get; private set; }
    public decimal TotalCostForeign => Quantity * UnitCostForeign;

    public static PurchaseOrderLineResult FromEntity(PurchaseOrderLine purchaseOrderLine) => new()
    {
        Id = purchaseOrderLine.Id,
        PurchaseOrderId = purchaseOrderLine.PurchaseOrderId,
        MaterialId = purchaseOrderLine.MaterialId,
        Quantity = purchaseOrderLine.Quantity,
        UnitCostForeign = purchaseOrderLine.UnitCostForeign
    };
}
