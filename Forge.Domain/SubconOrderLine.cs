using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class SubconOrderLine
{
    protected SubconOrderLine() { }
    public int Id { get; private set; }
    public int MaterialId { get; private set; }
    public decimal QuantitySent { get; private set; }
    public int ExpectedOutputMaterialId { get; private set; }
    public decimal ExpectedOutputQuantity { get; private set; }
    public decimal ProcessingCostForeign { get; private set; }
    public int SubconOrderId { get; private set; }
    public SubconOrder SubconOrder { get; private set; } = null!;
    public static SubconOrderLine Create(int materialId, decimal quantitySent, int expectedOutputMaterialId, decimal expectedOutputQuantity, decimal processingCostForeign)
    {
        if(quantitySent < 0)
            throw new DomainException("Quantity sent cannot be negative.");

        if(expectedOutputQuantity < 0)
            throw new DomainException("Expected output quantity cannot be negative.");

        if(processingCostForeign < 0)
            throw new DomainException("Processing cost in foreign currency cannot be negative.");

        return new SubconOrderLine
        {
            MaterialId = materialId,
            QuantitySent = quantitySent,
            ExpectedOutputMaterialId = expectedOutputMaterialId,
            ExpectedOutputQuantity = expectedOutputQuantity,
            ProcessingCostForeign = processingCostForeign
        };
    }

    public void Update(int materialId, decimal quantitySent, int expectedOutputMaterialId, decimal expectedOutputQuantity, decimal processingCostForeign)
    {
        if (quantitySent < 0)
            throw new DomainException("Quantity sent cannot be negative.");
        if (expectedOutputQuantity < 0)
            throw new DomainException("Expected output quantity cannot be negative.");
        if (processingCostForeign < 0)
            throw new DomainException("Processing cost in foreign currency cannot be negative.");
        MaterialId = materialId;
        QuantitySent = quantitySent;
        ExpectedOutputMaterialId = expectedOutputMaterialId;
        ExpectedOutputQuantity = expectedOutputQuantity;
        ProcessingCostForeign = processingCostForeign;
    }
}