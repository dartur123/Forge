using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
        // Rule 1: Quantity must be positive
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        // Rule 2: Lot must exist and be active
        var lot = await _context.Lots
            .Include(l => l.Material)
            .FirstOrDefaultAsync(l => l.Id == request.LotId);

        if (lot is null)
            throw new InvalidOperationException($"Lot {request.LotId} does not exist.");

        if (!lot.IsActive)
            throw new InvalidOperationException($"Lot {lot.LotNumber} is archived and cannot be used.");

        // Rule 3: For issuances, can't take more than available
        var isIssuance = request.Type == StockMovementType.IssuanceToProduction
            || request.Type == StockMovementType.IssuanceToCustomer
            || request.Type == StockMovementType.IssuanceToSubcon;

        if (isIssuance && request.Quantity > lot.Quantity)
            throw new InvalidOperationException(
                $"Insufficient stock. Requested: {request.Quantity}, Available: {lot.Quantity} {lot.Material.UnitOfMeasure}.");

        // Post the movement and update lot quantity atomically
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var movement = new StockMovement
            {
                Type = request.Type,
                LotId = request.LotId,
                FromLocationId = request.FromLocationId,
                ToLocationId = request.ToLocationId,
                JobReference = request.JobReference,
                Quantity = request.Quantity,
                UnitCostPhp = lot.UnitCostPhp,
                TotalCostPhp = request.Quantity * lot.UnitCostPhp,
                ReleasedByUserId = request.ReleasedByUserId,
                Timestamp = request.TransactionDate
            };

            _context.StockMovements.Add(movement);

            // Update lot quantity
            if (isIssuance)
                lot.Quantity -= request.Quantity;
            else if (request.Type == StockMovementType.ReceiptFromSupplier
                  || request.Type == StockMovementType.ReceiptFromProduction)
                lot.Quantity += request.Quantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new StockMovementResult
            {
                Id = movement.Id,
                LotId = lot.Id,
                LotNumber = lot.LotNumber,
                Quantity = request.Quantity,
                RemainingLotQuantity = lot.Quantity,
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
                                        .OrderBy(sm=> sm.Timestamp)
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
}

