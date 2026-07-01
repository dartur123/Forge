using Forge.Application.Requests;
using Forge.Application.Services;
using Forge.Domain;
using Forge.Domain.Enums;

namespace Forge.Tests;

public class StockLedgerServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public StockLedgerServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(Material material, Lot lot, User user)> SeedLotAsync(
        string sku, string lotNumber, decimal initialQuantity, decimal unitCost)
    {
        var material = Material.Create(sku, $"{sku} Material", MaterialType.Raw, null, "pcs");
        _fixture.DbContext.Materials.Add(material);
        await _fixture.DbContext.SaveChangesAsync();

        var lot = Lot.Create(lotNumber, material.Id, null, null, unitCost, DateTime.UtcNow, null);
        _fixture.DbContext.Lots.Add(lot);
        await _fixture.DbContext.SaveChangesAsync();

        var role = new Role { Name = "Warehouse Staff" };
        _fixture.DbContext.Roles.Add(role);
        await _fixture.DbContext.SaveChangesAsync();

        var user = new User
        {
            Name = "Test User",
            Email = $"{sku.ToLower()}@forge.com",
            PasswordHash = "placeholder",
            RoleId = role.Id,
            IsActive = true
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        if (initialQuantity > 0)
        {
            var service = new StockLedgerService(_fixture.DbContext);
            await service.PostMovementAsync(new PostStockMovementRequest
            {
                LotId = lot.Id,
                Quantity = initialQuantity,
                Type = StockMovementType.ReceiptFromSupplier,
                TransactionDate = DateTime.UtcNow
            });
        }

        return (material, lot, user);
    }

    // ---------- Quantity guard ----------

    [Fact]
    public async Task PostMovement_ShouldFail_WhenQuantityIsZero()
    {
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = 1,
            Quantity = 0,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));
    }

    [Fact]
    public async Task PostMovement_ShouldFail_WhenQuantityIsNegative()
    {
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = 1,
            Quantity = -5,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));
    }

    // ---------- Lot existence / status guards ----------

    [Fact]
    public async Task PostMovement_ShouldFail_WhenLotDoesNotExist()
    {
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = 999999,
            Quantity = 10,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));

        Assert.Contains("does not exist", ex.Message);
    }

    [Fact]
    public async Task PostMovement_ShouldFail_WhenLotIsArchived()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-ARC", "LOT-ARC", 100, 50);
        lot.Deactivate();
        await _fixture.DbContext.SaveChangesAsync();

        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 10,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));

        Assert.Contains("archived", ex.Message);
    }

    // ---------- Insufficient stock guard ----------

    [Fact]
    public async Task PostMovement_ShouldFail_WhenIssuanceExceedsAvailableStock()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-001", "LOT-001", 50, 100);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 100,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));
    }

    [Fact]
    public async Task PostMovement_ShouldAllowExactBalanceDeduction()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-EXACT", "LOT-EXACT", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 100,
            Type = StockMovementType.IssuanceToCustomer,
            TransactionDate = DateTime.UtcNow
        };

        await service.PostMovementAsync(request);
        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(0, quantityAfter);
    }

    // ---------- Increase / decrease correctness ----------

    [Fact]
    public async Task PostMovement_ShouldDeductLotQuantity_WhenIssuanceIsValid()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-002", "LOT-002", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 30,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        await service.PostMovementAsync(request);
        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(70, quantityAfter);
    }

    [Fact]
    public async Task PostMovement_ShouldIncreaseLotQuantity_WhenReceiptIsValid()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-RCV", "LOT-RCV", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 40,
            Type = StockMovementType.ReceiptFromSupplier,
            TransactionDate = DateTime.UtcNow
        };

        var result = await service.PostMovementAsync(request);
        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(140, quantityAfter);
        Assert.Equal(StockMovementType.ReceiptFromSupplier, result.Type);
    }

    [Fact]
    public async Task PostMovement_ShouldIncreaseLotQuantity_WhenAdjustmentIncrease()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-ADJ-INC", "LOT-ADJ-INC", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 10,
            Type = StockMovementType.AdjustmentIncrease,
            TransactionDate = DateTime.UtcNow
        };

        await service.PostMovementAsync(request);
        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(110, quantityAfter);
    }

    [Fact]
    public async Task PostMovement_ShouldDecreaseLotQuantity_WhenAdjustmentDecrease()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-ADJ-DEC", "LOT-ADJ-DEC", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 10,
            Type = StockMovementType.AdjustmentDecrease,
            TransactionDate = DateTime.UtcNow
        };

        await service.PostMovementAsync(request);
        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(90, quantityAfter);
    }

    [Fact]
    public async Task PostMovement_ShouldNotAffectQuantity_WhenTypeIsTransfer()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-TRF", "LOT-TRF", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 20,
            Type = StockMovementType.Transfer,
            TransactionDate = DateTime.UtcNow
        };

        await service.PostMovementAsync(request);
        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(100, quantityAfter);
    }

    [Fact]
    public async Task PostMovement_ShouldAccumulateCorrectly_AcrossMultipleMovements()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-MULTI", "LOT-MULTI", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        await service.PostMovementAsync(new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 50,
            Type = StockMovementType.ReceiptFromProduction,
            TransactionDate = DateTime.UtcNow
        });

        await service.PostMovementAsync(new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 30,
            Type = StockMovementType.IssuanceToCustomer,
            TransactionDate = DateTime.UtcNow
        });

        await service.PostMovementAsync(new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 20,
            Type = StockMovementType.Transfer,
            TransactionDate = DateTime.UtcNow
        });

        var quantityAfter = await service.GetLotCurrentQuantity(lot.Id);

        // 100 (seed) + 50 (receipt) - 30 (issuance); transfer is neutral
        Assert.Equal(120, quantityAfter);
    }

    // ---------- GetLotCurrentQuantity ----------

    [Fact]
    public async Task GetLotCurrentQuantity_ShouldReturnZero_WhenNoMovementsExist()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-NEW", "LOT-NEW", 0, 100);
        var service = new StockLedgerService(_fixture.DbContext);

        var quantity = await service.GetLotCurrentQuantity(lot.Id);

        Assert.Equal(0, quantity);
    }

    // ---------- GetLotHistoryAsync ----------

    [Fact]
    public async Task GetLotHistoryAsync_ShouldFail_WhenLotDoesNotExist()
    {
        var service = new StockLedgerService(_fixture.DbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetLotHistoryAsync(999999));
    }

    [Fact]
    public async Task GetLotHistoryAsync_ShouldReturnEmptyList_WhenNoMovementsExist()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-EMPTY", "LOT-EMPTY", 0, 100);
        var service = new StockLedgerService(_fixture.DbContext);

        var history = await service.GetLotHistoryAsync(lot.Id);

        Assert.Empty(history);
    }

    [Fact]
    public async Task GetLotHistoryAsync_ShouldReturnMovementsInChronologicalOrder()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-HIST", "LOT-HIST", 100, 50);
        var service = new StockLedgerService(_fixture.DbContext);

        await service.PostMovementAsync(new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 10,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        });

        await service.PostMovementAsync(new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 20,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        });

        var history = await service.GetLotHistoryAsync(lot.Id);

        Assert.Equal(3, history.Count); // includes the seed receipt
        Assert.True(history[0].Timestamp <= history[1].Timestamp);
        Assert.True(history[1].Timestamp <= history[2].Timestamp);
    }

    [Fact]
    public async Task GetLotHistoryAsync_ShouldContainCorrectMovementDetails()
    {
        var (_, lot, _) = await SeedLotAsync("SKU-DETAIL", "LOT-DETAIL", 0, 25);
        var service = new StockLedgerService(_fixture.DbContext);

        await service.PostMovementAsync(new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 15,
            Type = StockMovementType.ReceiptFromSupplier,
            TransactionDate = DateTime.UtcNow,
            JobReference = "JOB-001"
        });

        var history = await service.GetLotHistoryAsync(lot.Id);

        Assert.Single(history);
        Assert.Equal(15, history[0].Quantity);
        Assert.Equal(25, history[0].UnitCostPhp);
        Assert.Equal("JOB-001", history[0].JobReference);
        Assert.Equal(StockMovementType.ReceiptFromSupplier.ToString(), history[0].Type);
    }
}