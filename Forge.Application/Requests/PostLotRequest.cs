using Forge.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Forge.Application.Requests
{
    public class PostLotRequest
    {
        [MinLength(1)]
        public string LotNumber { get; set; } = string.Empty;
        public int MaterialId { get; set; }
        public int? SupplierId { get; set; }
        public int? CurrentLocationId { get; set; }
        public decimal UnitCostPhp { get; set; }
        public decimal Quantity { get; set; }
        public StockMovementType StockMovementType { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
