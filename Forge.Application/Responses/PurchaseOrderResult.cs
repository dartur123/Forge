using Forge.Domain.Enums;

namespace Forge.Application.Responses;

public class PurchaseOrderResult
{
    public int Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public int SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public string Currency { get; private set; } = "PHP";
    public decimal ExchangeRate { get; private set; } = 1.0m;
    public int CreatedByUserId { get; private set; }
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public bool IsSentToSupplier { get; private set; } = false;
    public decimal TotalCostForeign => Lines.Sum(line => line.TotalCostForeign);

    public List<PurchaseOrderLineResult> Lines { get; private set; } = new();

    public static PurchaseOrderResult FromEntity(Domain.PurchaseOrder purchaseOrder) => new()
    {
        Id = purchaseOrder.Id,
        OrderNumber = purchaseOrder.OrderNumber,
        SupplierId = purchaseOrder.SupplierId,
        Status = purchaseOrder.Status,
        Currency = purchaseOrder.Currency,
        ExchangeRate = purchaseOrder.ExchangeRate,
        CreatedByUserId = purchaseOrder.CreatedByUserId,
        CreatedDate = purchaseOrder.CreatedDate,
        IsSentToSupplier = purchaseOrder.IsSentToSupplier,
        Lines = purchaseOrder.Lines.Select(line => PurchaseOrderLineResult.FromEntity(line)).ToList()
    };
}
