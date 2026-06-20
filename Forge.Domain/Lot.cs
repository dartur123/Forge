using Forge.Domain.Enums;

namespace Forge.Domain;

public class Lot
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public int MaterialId { get; set; }
    public int? SupplierId { get; set; }
    public int? CurrentLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCostPhp { get; set; }
    public decimal TotalCostPhp { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public LotStatus Status { get; set; } = LotStatus.Active;
}