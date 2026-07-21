using System.ComponentModel.DataAnnotations;

namespace Forge.Application.Requests;

public class PostPurchaseOrderRequest
{
    [MinLength(1)]
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string Currency { get; set; } = "PHP";
    public decimal ExchangeRate { get; set; } = 1.0m;
    public int CreatedByUserId { get; set; }

    public List<PostPurchaseOrderLineRequest>? Lines { get; set; }
}
