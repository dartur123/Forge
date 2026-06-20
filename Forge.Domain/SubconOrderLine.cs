namespace Forge.Domain;

public class SubconOrderLine
{
    public int Id { get; set; }
    public int SubconOrderId { get; set; }
    public int MaterialSentId { get; set; }
    public decimal QuantitySent { get; set; }
    public int ExpectedOutputMaterialId { get; set; }
    public decimal ExpectedOutputQuantity { get; set; }
    public decimal ProcessingCostForeign { get; set; }
    public decimal ProcessingCostPhp { get; set; }
}