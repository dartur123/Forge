using Forge.Application.Requests;
using Forge.Application.Responses;

namespace Forge.Application.Interfaces;

public interface IStockLedgerService
{
    Task<StockMovementResult> PostMovementAsync(PostStockMovementRequest request);
    Task<List<StockMovementHistoryItem>> GetLotHistoryAsync(int lotId);
    Task<decimal> GetLotCurrentQuantityAsync(int lotId);
    Task<Dictionary<int, decimal>> GetLotQuantitiesAsync(List<int> lotIds);
    Task<StockMovementResult> PostMovementWithinTransactionAsync(PostStockMovementRequest request);
}
