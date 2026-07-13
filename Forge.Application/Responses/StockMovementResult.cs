using Forge.Domain.Enums;

namespace Forge.Application.Responses;

public class StockMovementResult
{
    public int Id { get; set; }
    public int LotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCostPhp { get; set; }
    public decimal TotalCostPhp => UnitCostPhp * Quantity;
    public StockMovementType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? JobReference { get; set; }
}