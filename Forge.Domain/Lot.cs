using Forge.Domain.Enums;

namespace Forge.Domain;

public class Lot
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public int MaterialId { get; set; }
    public Material Material { get; set; } = null!;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int? CurrentLocationId { get; set; }
    public Location? CurrentLocation { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCostPhp { get; set; }
    public decimal TotalCostPhp { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public LotStatus Status { get; set; } = LotStatus.Active;
    public bool IsActive { get; set; } = true;
}