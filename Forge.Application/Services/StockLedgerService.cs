using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services;

public class StockLedgerService : IStockLedgerService
{
    private readonly ForgeDbContext _context;

    public StockLedgerService(ForgeDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovementResult> PostMovementAsync(PostStockMovementRequest request)
    {
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Lots\" WHERE \"Id\" = {0} FOR UPDATE", request.LotId);

            var lot = await _context.Lots
                .Include(l => l.Material)
                .FirstOrDefaultAsync(l => l.Id == request.LotId);

            if (lot is null)
                throw new InvalidOperationException($"Lot {request.LotId} does not exist.");

            if (!lot.IsActive)
                throw new InvalidOperationException($"Lot {lot.LotNumber} is archived and cannot be used.");

            decimal lotQuantity = await GetLotCurrentQuantity(request.LotId);
            if (request.Type.IsDecrease() && request.Quantity > lotQuantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Requested: {request.Quantity}, Available: {lotQuantity} {lot.Material.UnitOfMeasure}.");
            }

            var movement = StockMovement.Create(
                request.Type,
                request.LotId,
                request.FromLocationId,
                request.ToLocationId,
                request.JobReference,
                request.Quantity,
                lot.UnitCostPhp,
                request.ReleasedByUserId,
                request.ReceivedByUserId);

            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new StockMovementResult
            {
                Id = movement.Id,
                LotId = lot.Id,
                LotNumber = lot.LotNumber,
                Quantity = request.Quantity,
                UnitCostPhp = lot.UnitCostPhp,
                Type = request.Type,
                TransactionDate = request.TransactionDate,
                JobReference = request.JobReference
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<StockMovementHistoryItem>> GetLotHistoryAsync(int lotId)
    {
        var lotExists = await _context.Lots.AnyAsync(l => l.Id == lotId);
        if (!lotExists)
            throw new InvalidOperationException($"Lot {lotId} does not exist.");

        var stockMovementList = await _context.StockMovements
                                        .Include(sm => sm.Lot)
                                        .Include(sm => sm.FromLocation)
                                        .Include(sm => sm.ToLocation)
                                        .Include(sm => sm.ReleasedByUser)
                                        .Include(sm => sm.ReceivedByUser)
                                        .Where(sm => sm.LotId == lotId)
                                        .OrderBy(sm => sm.Timestamp)
                                        .ToListAsync();

        return stockMovementList.Select(sm => new StockMovementHistoryItem
        {
            Id = sm.Id,
            Type = sm.Type.ToString(),
            Quantity = sm.Quantity,
            UnitCostPhp = sm.UnitCostPhp,
            TotalCostPhp = sm.TotalCostPhp,
            JobReference = sm.JobReference,
            FromLocation = sm.FromLocation is null ? null : LocationSummary.FromEntity(sm.FromLocation),
            ToLocation = sm.ToLocation is null ? null : LocationSummary.FromEntity(sm.ToLocation),
            ReleasedByUserId = sm.ReleasedByUserId,
            ReceivedByUserId = sm.ReceivedByUserId,
            Timestamp = sm.Timestamp
        }).ToList();
    }

    public async Task<decimal> GetLotCurrentQuantity(int lotId)
    {
        var movements = await _context.StockMovements
            .Where(sm => sm.LotId == lotId)
            .ToListAsync();

        var increase = movements.Where(sm => sm.Type.IsIncrease()).Sum(sm => sm.Quantity);
        var decrease = movements.Where(sm => sm.Type.IsDecrease()).Sum(sm => sm.Quantity);

        return increase - decrease;
    }
}

