using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class PurchaseOrderLine
{
    protected PurchaseOrderLine() { }
    public int Id { get; private set; }
    public int PurchaseOrderId { get; private set; }
    public int MaterialId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCostForeign { get; private set; }
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public static PurchaseOrderLine Create(int purchaseOrderId, int materialId, decimal quantity, decimal unitCostForeign)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitCostForeign < 0)
            throw new DomainException("Unit cost in foreign currency cannot be negative.");
        return new PurchaseOrderLine
        {
            PurchaseOrderId = purchaseOrderId,
            MaterialId = materialId,
            Quantity = quantity,
            UnitCostForeign = unitCostForeign
        };
    }
    public void Update(int materialId, decimal quantity, decimal unitCostForeign)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitCostForeign < 0)
            throw new DomainException("Unit cost in foreign currency cannot be negative.");
        MaterialId = materialId;
        Quantity = quantity;
        UnitCostForeign = unitCostForeign;
    }
}