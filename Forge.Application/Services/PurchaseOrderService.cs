using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly ForgeDbContext _context;
    public PurchaseOrderService(ForgeDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrderResult> CreateAsync(PostPurchaseOrderRequest request)
    {
        var purchaseOrder = PurchaseOrder.Create(request.OrderNumber, request.SupplierId, request.Currency, request.ExchangeRate, request.CreatedByUserId);
        request.Lines?.ForEach(lineRequest =>
        {
            var line = PurchaseOrderLine.Create(lineRequest.MaterialId, lineRequest.Quantity, lineRequest.UnitCostForeign);
            purchaseOrder.AddLine(line);
        });
        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync();

        return PurchaseOrderResult.FromEntity(purchaseOrder);
    }

    public async Task<PurchaseOrderResult> GetByIdAsync(int purchaseOrderId)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);
        if (purchaseOrder == null)
        {
            throw new KeyNotFoundException($"Purchase order with ID {purchaseOrderId} not found.");
        }
        return PurchaseOrderResult.FromEntity(purchaseOrder);
    }

    public async Task<List<PurchaseOrderResult>> GetAllAsync(bool includeLines)
    {
        var query = _context.PurchaseOrders.AsQueryable();
        if (includeLines)
        {
            query = query.Include(po => po.Lines);
        }
        
        return (await query.ToListAsync()).Select(PurchaseOrderResult.FromEntity).ToList();
    }
}
