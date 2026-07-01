namespace Forge.Application.Responses
{
    public class StockMovementHistoryItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCostPhp { get; set; }
        public decimal TotalCostPhp { get; set; }
        public string? JobReference { get; set; }
        public LocationSummary? FromLocation { get; set; }
        public LocationSummary? ToLocation { get; set; }
        public int? ReleasedByUserId { get; set; }
        public int? ReceivedByUserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
