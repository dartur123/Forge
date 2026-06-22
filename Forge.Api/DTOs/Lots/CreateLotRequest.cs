using Forge.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Forge.Api.DTOs.Lots
{
    public class CreateLotRequest
    {
        [Required]
        [MinLength(1)]
        public string LotNumber { get; set; } = string.Empty;
        public int MaterialId { get; set; }
        public int? SupplierId { get; set; }
        public int? CurrentLocationId { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0")]
        public decimal UnitCostPhp { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
