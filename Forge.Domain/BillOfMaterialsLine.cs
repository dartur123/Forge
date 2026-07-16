using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class BillOfMaterialsLine
{
    protected BillOfMaterialsLine() { }
    public int Id { get; private set; }
    public int BillOfMaterialsId { get; private set; }
    public BillOfMaterials BillOfMaterials { get; private set; } = null!;
    public int MaterialId { get; private set; }
    public Material Material { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public static BillOfMaterialsLine Create(int materialId, decimal quantity, string unitOfMeasure)
    {
        if (materialId <= 0)
            throw new DomainException("Material id is required.");
        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new DomainException("Unit of measure is required.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0.");

        return new BillOfMaterialsLine
        {
            MaterialId = materialId,
            Quantity = quantity,
            UnitOfMeasure = unitOfMeasure
        };
    }

    public void Update(int materialId, decimal quantity, string unitOfMeasure)
    {
        if (materialId <= 0)
            throw new DomainException("Material id is required.");
        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new DomainException("Unit of measure is required.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0.");

        MaterialId = materialId;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
    }
}