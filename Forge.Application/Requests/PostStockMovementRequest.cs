using Forge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forge.Application.Requests;

public class PostStockMovementRequest
{
    public int LotId { get; set; }
    public decimal Quantity { get; set; }
    public StockMovementType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public int ReleasedByUserId { get; set; }
    public string? JobReference { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
}

