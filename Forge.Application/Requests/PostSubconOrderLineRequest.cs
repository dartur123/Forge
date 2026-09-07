namespace Forge.Application.Requests;

public class PostSubconOrderLineRequest
{
    public int MaterialId { get; set; }
    public decimal QuantitySent { get; set; }
    public int ExpectedOutputMaterialId { get; set; }
    public decimal ExpectedOutputQuantity { get; set; }
    public decimal ProcessingCostForeign { get; set; }
}