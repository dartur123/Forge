using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services
{
    public class LotService : ILotService
    {
        private readonly ForgeDbContext _context;
        private readonly IStockLedgerService _stockLedgerService;
        public LotService(ForgeDbContext context, IStockLedgerService stockLedgerService)
        {
            _context = context;
            _stockLedgerService = stockLedgerService;
        }

        public async Task<LotResult> CreateLotAsync(PostLotRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var lot = Lot.Create(
                    request.LotNumber,
                    request.MaterialId,
                    request.SupplierId,
                    request.CurrentLocationId,
                    request.UnitCostPhp,
                    request.ReceivedDate,
                    request.ExpiryDate);

                _context.Lots.Add(lot);
                await _context.SaveChangesAsync();

                if (request.Quantity > 0)
                {
                    await _stockLedgerService.PostMovementWithinTransactionAsync(new PostStockMovementRequest
                    {
                        LotId = lot.Id,
                        Quantity = request.Quantity,
                        Type = request.StockMovementType,
                        TransactionDate = request.ReceivedDate
                    });
                }

                await transaction.CommitAsync();

                var lotWithIncludes = await _context.Lots
                    .Include(l => l.Material)
                    .Include(l => l.Supplier)
                    .Include(l => l.CurrentLocation)
                    .FirstAsync(l => l.Id == lot.Id);

                var quantity = await _stockLedgerService.GetLotCurrentQuantity(lot.Id);
                return LotResult.FromEntity(lotWithIncludes, quantity);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeactivateLotAsync(int lotId)
        {
            var lot = await _context.Lots.FirstOrDefaultAsync(l => l.Id == lotId);
            if(lot==null)
                throw new NotFoundException($"Lot {lotId} not found");
            if(!lot.IsActive)
                throw new InvalidOperationException($"Lot {lotId} is already deactivated");
            lot.Deactivate();
            await _context.SaveChangesAsync();
        }

        public async Task<List<LotResult>> GetAllLotsAsync()
        {
            var lots = await _context.Lots.Include(l => l.Material)
                                          .Include(l => l.Supplier)
                                          .Include(l => l.CurrentLocation)
                                          .ToListAsync();
            if (lots == null || lots.Count == 0)
                return new();

            var lotquantities = await _stockLedgerService.GetLotQuantitiesAsync(lots.Select(l=>l.Id).ToList());

            return lots.Select(l => LotResult.FromEntity(l, lotquantities[l.Id])).ToList();
        }

        public async Task<LotResult> GetLotByIdAsync(int lotId)
        {
            var lot = await _context.Lots.Include(l=>l.Material)
                                         .Include(l=>l.Supplier)
                                         .Include(l => l.CurrentLocation)
                                        .Where(l=>l.Id == lotId).FirstOrDefaultAsync();
            if(lot==null)
                throw new NotFoundException($"Lot {lotId} not found");

            decimal quantity = await _stockLedgerService.GetLotCurrentQuantity(lotId);

            return LotResult.FromEntity(lot,quantity);
        }

        public async Task<LotResult> UpdateLotAsync(int lotId, PostLotRequest request)
        {
            var lot = await _context.Lots.FirstOrDefaultAsync(l => l.Id == lotId);
            if (lot == null)
                throw new NotFoundException($"Lot {lotId} not found");
            if (!lot.IsActive)
                throw new InvalidOperationException($"Lot {lotId} is deactivated");

            lot.Update(request.LotNumber, 
                request.MaterialId, 
                request.SupplierId, 
                request.CurrentLocationId, 
                request.UnitCostPhp, 
                request.ReceivedDate, 
                request.ExpiryDate);

            await _context.SaveChangesAsync();

            var lotWithIncludes = await _context.Lots.Include(l => l.Material)
                                                    .Include(l => l.Supplier)
                                                    .Include(l => l.CurrentLocation)
                                                    .FirstAsync(l => l.Id == lot.Id);

            var quantity = await _stockLedgerService.GetLotCurrentQuantity(lotId);
            return LotResult.FromEntity(lotWithIncludes, quantity);
        }
    }
}
