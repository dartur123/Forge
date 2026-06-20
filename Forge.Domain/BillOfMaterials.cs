namespace Forge.Domain;

public class BillOfMaterials
{
    public int Id { get; set; }
    public int FinishedGoodMaterialId { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public List<BillOfMaterialsLine> Lines { get; set; } = new();
}