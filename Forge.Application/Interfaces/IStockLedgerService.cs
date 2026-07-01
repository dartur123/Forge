using Forge.Application.Requests;
using Forge.Application.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forge.Application.Interfaces;

public interface IStockLedgerService
{
    Task<StockMovementResult> PostMovementAsync(PostStockMovementRequest request);
    Task<List<StockMovementHistoryItem>> GetLotHistoryAsync(int lotId);
    Task<decimal> GetLotCurrentQuantity(int lotId);
}
