using Forge.Domain.Enums;

namespace Forge.Domain;

public class SubconOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SubcontractorId { get; set; }
    public SubconOrderStatus Status { get; set; } = SubconOrderStatus.Draft;
    public string Currency { get; set; } = "PHP";
    public decimal ExchangeRate { get; set; } = 1.0m;
    public decimal TotalAmountForeign { get; set; }
    public decimal TotalAmountPhp { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public List<SubconOrderLine> Lines { get; set; } = new();
}