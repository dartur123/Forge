using Forge.Application.Requests;
using Forge.Application.Responses;

namespace Forge.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderResult> CreateAsync(PostPurchaseOrderRequest request);
    Task<List<PurchaseOrderResult>> GetAllAsync(bool includeLines);
    Task<PurchaseOrderResult> GetByIdAsync(int purchaseOrderId);
}
