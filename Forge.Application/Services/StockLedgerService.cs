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
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = await PostMovementWithinTransactionAsync(request);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    /// <summary>
    /// Saves the inventory movement to the database within an existing transaction. This method should be called only when a transaction is already in progress.
    /// </summary>
    /// <param name="request">
    /// Contains the details of the stock movement to be saved, including the lot ID, quantity, movement type, transaction date, user IDs for release and receipt, job reference, and location IDs.
    /// </param>
    /// <returns>
    /// Returns a <see cref="StockMovementResult"/> object containing the details of the saved stock movement.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Occurs if the method is called outside of an existing transaction, if the quantity is less than or equal to zero, if the specified lot does not exist, or if there is insufficient stock for a decrease movement.
    /// </exception>
    public async Task<StockMovementResult> PostMovementWithinTransactionAsync(PostStockMovementRequest request)
    {
        if (_context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("PostMovementWithinTransactionAsync must be called within an existing transaction.");

        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"Lots\" WHERE \"Id\" = {0} FOR UPDATE", request.LotId);

        var lot = await _context.Lots
            .Include(l => l.Material)
            .FirstOrDefaultAsync(l => l.Id == request.LotId);

        if (lot is null)
            throw new InvalidOperationException($"Lot {request.LotId} does not exist.");

        decimal lotQuantity = await GetLotCurrentQuantityAsync(request.LotId);
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
            FromLocation = sm.FromLocation is null ? null : LocationResult.FromEntity(sm.FromLocation),
            ToLocation = sm.ToLocation is null ? null : LocationResult.FromEntity(sm.ToLocation),
            ReleasedByUserId = sm.ReleasedByUserId,
            ReceivedByUserId = sm.ReceivedByUserId,
            Timestamp = sm.Timestamp
        }).ToList();
    }

    public async Task<decimal> GetLotCurrentQuantityAsync(int lotId)
    {
        var movements = await _context.StockMovements
            .Where(sm => sm.LotId == lotId)
            .ToListAsync();

        var increase = movements.Where(sm => sm.Type.IsIncrease()).Sum(sm => sm.Quantity);
        var decrease = movements.Where(sm => sm.Type.IsDecrease()).Sum(sm => sm.Quantity);

        return increase - decrease;
    }

    public async Task<Dictionary<int, decimal>> GetLotQuantitiesAsync(List<int> lotIds)
    {
        var movements = await _context.StockMovements
            .Where(sm => lotIds.Contains(sm.LotId))
            .ToListAsync();

        return lotIds.ToDictionary(
            lotId => lotId,
            lotId =>
            {
                var lotMovements = movements.Where(sm => sm.LotId == lotId).ToList();
                var increase = lotMovements.Where(sm => sm.Type.IsIncrease()).Sum(sm => sm.Quantity);
                var decrease = lotMovements.Where(sm => sm.Type.IsDecrease()).Sum(sm => sm.Quantity);
                return increase - decrease;
            });
    }
}

