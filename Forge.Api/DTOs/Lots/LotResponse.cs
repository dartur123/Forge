using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Api.DTOs.Materials;
using Forge.Api.DTOs.Suppliers;
using Forge.Api.DTOs.Location;

namespace Forge.Api.DTOs.Lots
{
    public class LotResponse
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public MaterialResponse Material { get; set; } = new();
        public SupplierResponse? Supplier { get; set; }
        public LocationResponse? Location { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static LotResponse FromEntity(Lot lot,decimal qty) => new()
        {
            Id = lot.Id,
            LotNumber = lot.LotNumber,
            Material = MaterialResponse.FromEntity(lot.Material),
            Supplier = lot.Supplier is null
                ? null
                : SupplierResponse.FromEntity(lot.Supplier),
            Location = lot.CurrentLocation is null
                ? null
                : LocationResponse.FromEntity(lot.CurrentLocation),
            Quantity = qty,
            UnitCost = lot.UnitCostPhp,
            TotalCost = lot.UnitCostPhp * qty,
            ReceivedDate = lot.ReceivedDate,
            ExpiryDate = lot.ExpiryDate,
            Status = lot.Status.ToString(),
            IsActive = lot.IsActive
        };
    }
}