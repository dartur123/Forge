using Forge.Domain.Enums;

namespace Forge.Domain;

public class StockMovement
{
    public int Id { get; set; }
    public StockMovementType Type { get; set; }
    public int LotId { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public string? JobReference { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCostPhp { get; set; }
    public decimal TotalCostPhp { get; set; }
    public int ReleasedByUserId { get; set; }
    public int? ReceivedByUserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}