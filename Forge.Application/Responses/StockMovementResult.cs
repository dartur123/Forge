using Forge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forge.Application.Responses;

public class StockMovementResult
{
    public int Id { get; set; }
    public int LotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal RemainingLotQuantity { get; set; }
    public StockMovementType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? JobReference { get; set; }
}