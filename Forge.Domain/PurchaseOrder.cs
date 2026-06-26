using Forge.Domain.Enums;

namespace Forge.Domain;

public class PurchaseOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public string Currency { get; set; } = "PHP";
    public decimal ExchangeRate { get; set; } = 1.0m;
    public decimal TotalAmountForeign { get; set; }
    public decimal TotalAmountPhp { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public List<PurchaseOrderLine> Lines { get; set; } = new();
}
