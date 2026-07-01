using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class StockMovement
{
    protected StockMovement() { }
    public int Id { get; private set; }
    public StockMovementType Type { get; private set; }
    public int LotId { get; private set; }
    public Lot Lot { get; private set; } = null!;
    public int? FromLocationId { get; private set; }
    public Location? FromLocation { get; private set; }
    public int? ToLocationId { get; private set; }
    public Location? ToLocation { get; private set; }
    public string? JobReference { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCostPhp { get; private set; }
    public decimal TotalCostPhp { get; private set; }
    public int? ReleasedByUserId { get; private set; }
    public User? ReleasedByUser { get; private set; } = null!;
    public int? ReceivedByUserId { get; private set; }
    public User? ReceivedByUser { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public static StockMovement Create(StockMovementType type, int lotId, int? fromLocationId, int? toLocationId, string? jobReference, decimal quantity, decimal unitCostPhp, int? releasedByUserId, int? receivedByUserId)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitCostPhp < 0)
            throw new DomainException("Unit cost must be non-negative.");
        return new StockMovement
        {
            Type = type,
            LotId = lotId,
            FromLocationId = fromLocationId,
            ToLocationId = toLocationId,
            JobReference = jobReference,
            Quantity = quantity,
            UnitCostPhp = unitCostPhp,
            TotalCostPhp = quantity * unitCostPhp,
            ReleasedByUserId = releasedByUserId,
            ReceivedByUserId = receivedByUserId
        };
    }
}