using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly ForgeDbContext _context;
    private readonly IApprovalService _approvalService;
    public PurchaseOrderService(ForgeDbContext context, IApprovalService approvalService)
    {
        _context = context;
        _approvalService = approvalService;
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

    public async Task SubmitAsync(int purchaseOrderId)
    {
        var searchedPurchaseOrder = await _context.PurchaseOrders
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

        if (searchedPurchaseOrder == null)
            throw new NotFoundException("Purchase order not found.");

        if(searchedPurchaseOrder.Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only draft purchase orders can be submitted.");

        var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            searchedPurchaseOrder.Submit();
            await _context.SaveChangesAsync();

            await _approvalService.StartApprovalAsync(nameof(PurchaseOrder), searchedPurchaseOrder.Id);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
