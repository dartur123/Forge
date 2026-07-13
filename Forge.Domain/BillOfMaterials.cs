using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class BillOfMaterials
{
    protected BillOfMaterials() { }
    public int Id { get; private set; }
    public int OutputMaterialId { get; private set; }
    public Material OutputMaterial { get; private set; } = null!;
    public BillOfMaterialStatus BillOfMaterialStatus { get; private set; }
    public bool IsActive { get; private set; }

    public List<BillOfMaterialsLine> Lines { get; private set; } = new();

    public static BillOfMaterials Create(int outputMaterialId)
    {
        if (outputMaterialId <= 0)
            throw new DomainException("Output material id is required.");
        return new BillOfMaterials
        {
            OutputMaterialId = outputMaterialId,
            BillOfMaterialStatus = BillOfMaterialStatus.Draft,
            IsActive = true
        };
    }

    public void AddLine(int materialId, decimal quantity, string unitOfMeasure)
    {
        if(BillOfMaterialStatus == BillOfMaterialStatus.Approved)
            throw new DomainException("Cannot add line to an approved bill of materials.");

        if (OutputMaterialId == materialId)
            throw new DomainException("Output material cannot be used as input material.");

        var line = BillOfMaterialsLine.Create(Id, materialId, quantity, unitOfMeasure);
        Lines.Add(line);
        if (BillOfMaterialStatus == BillOfMaterialStatus.Rejected)
            BillOfMaterialStatus = BillOfMaterialStatus.Draft;
    }

    public void UpdateLine(int lineId, int materialId, decimal quantity, string unitOfMeasure)
    {
        if(BillOfMaterialStatus == BillOfMaterialStatus.Approved)
            throw new DomainException("Cannot update line of an approved bill of materials.");

        if (OutputMaterialId == materialId)
            throw new DomainException("Output material cannot be used as input material.");

        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            throw new DomainException("Line not found.");
        line.Update(materialId, quantity, unitOfMeasure);
        if (BillOfMaterialStatus == BillOfMaterialStatus.Rejected)
            BillOfMaterialStatus = BillOfMaterialStatus.Draft;
    }

    public void RemoveLine(int lineId)
    {
        if(BillOfMaterialStatus == BillOfMaterialStatus.Approved)
            throw new DomainException("Cannot remove line from an approved bill of materials.");

        var line = Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
            throw new DomainException("Line not found.");
        Lines.Remove(line);
        if (BillOfMaterialStatus == BillOfMaterialStatus.Rejected)
            BillOfMaterialStatus = BillOfMaterialStatus.Draft;
    }

    public void Approve()
    {
        if(BillOfMaterialStatus != BillOfMaterialStatus.Draft)
            throw new DomainException("Only draft bill of materials can be approved.");

        if (Lines.Count == 0)
            throw new DomainException("Cannot approve a bill of materials with no lines.");

        BillOfMaterialStatus = BillOfMaterialStatus.Approved;
    }

    public void Reject()
    {
        if (BillOfMaterialStatus != BillOfMaterialStatus.Draft)
            throw new DomainException("Only draft bill of materials can be rejected.");

        BillOfMaterialStatus = BillOfMaterialStatus.Rejected;
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}