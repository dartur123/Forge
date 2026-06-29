using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class Lot
{
    protected Lot() { }
    public int Id { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public int MaterialId { get; private set; }
    public Material Material { get; private set; } = null!;
    public int? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public int? CurrentLocationId { get; private set; }
    public Location? CurrentLocation { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCostPhp { get; private set; }
    public decimal TotalCostPhp { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public LotStatus Status { get; private set; } = LotStatus.Active;
    public bool IsActive { get; private set; } = true;

    public static Lot Create(string lotNumber, int materialId, int? supplierId, int? currentLocationId, decimal quantity, decimal unitCostPhp, DateTime receivedDate, DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new DomainException("Lot number is required.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitCostPhp < 0)
            throw new DomainException("Unit cost must be non-negative.");
        return new Lot
        {
            LotNumber = lotNumber,
            MaterialId = materialId,
            SupplierId = supplierId,
            CurrentLocationId = currentLocationId,
            Quantity = quantity,
            UnitCostPhp = unitCostPhp,
            TotalCostPhp = quantity * unitCostPhp,
            ReceivedDate = receivedDate,
            ExpiryDate = expiryDate
        };
    }

    public void Update(string lotNumber, int materialId, int? supplierId, int? currentLocationId, decimal quantity, decimal unitCostPhp, DateTime receivedDate, DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(lotNumber))
            throw new DomainException("Lot number is required.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitCostPhp < 0)
            throw new DomainException("Unit cost must be non-negative.");
        LotNumber = lotNumber;
        MaterialId = materialId;
        SupplierId = supplierId;
        CurrentLocationId = currentLocationId;
        Quantity = quantity;
        UnitCostPhp = unitCostPhp;
        TotalCostPhp = quantity * unitCostPhp;
        ReceivedDate = receivedDate;
        ExpiryDate = expiryDate;
    }

    public void Deduct(decimal quantity) => Quantity -= quantity;
    public void Add(decimal quantity) => Quantity += quantity;
    public void Deactivate() => IsActive = false;
}