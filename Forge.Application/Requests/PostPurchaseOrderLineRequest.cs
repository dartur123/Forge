using System.ComponentModel.DataAnnotations;

namespace Forge.Application.Requests;

public class PostPurchaseOrderLineRequest
{
    [Range(1,int.MaxValue)]
    public int MaterialId { get; set; }

    [Range(1,int.MaxValue)]
    public decimal Quantity { get; set; }

    [Range(1,int.MaxValue)]
    public decimal UnitCostForeign { get; set; }
}
