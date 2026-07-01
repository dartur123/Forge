using Forge.Domain;
using Forge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forge.Application.Responses
{
    public class LotResult
    {
        public int Id { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public MaterialResult Material { get; set; } = null!;
        public SupplierResult? Supplier { get; set; }
        public LocationResult? CurrentLocation { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCostPhp { get; set; }
        public DateTime ReceivedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public LotStatus Status { get; set; } = LotStatus.Active;
        public bool IsActive { get; set; }

        public static LotResult FromEntity(Lot lot, decimal quantity)
        {
            return new LotResult
            {
                Id = lot.Id,
                LotNumber = lot.LotNumber,
                Material = MaterialResult.FromEntity(lot.Material),
                Supplier = lot.Supplier is null ? null : SupplierResult.FromEntity(lot.Supplier),
                CurrentLocation = lot.CurrentLocation is null ? null : LocationResult.FromEntity(lot.CurrentLocation),
                Quantity = quantity,
                UnitCostPhp = lot.UnitCostPhp,
                ReceivedDate = lot.ReceivedDate,
                ExpiryDate = lot.ExpiryDate,
                Status = lot.Status,
                IsActive = lot.IsActive
            };
        }
    }
}
