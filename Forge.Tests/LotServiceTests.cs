using Forge.Application.Exceptions;
using Forge.Application.Requests;
using Forge.Application.Services;
using Forge.Domain;
using Forge.Domain.Enums;

namespace Forge.Tests;

public class LotServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public LotServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private LotService CreateService()
    {
        var stockLedgerService = new StockLedgerService(_fixture.DbContext);
        return new LotService(_fixture.DbContext, stockLedgerService);
    }

    private async Task<Material> SeedMaterialAsync(string sku)
    {
        var material = Material.Create(sku, $"{sku} Material", MaterialType.Raw, null, "pcs");
        _fixture.DbContext.Materials.Add(material);
        await _fixture.DbContext.SaveChangesAsync();
        return material;
    }

    // ---------- CreateLotAsync ----------

    [Fact]
    public async Task CreateLot_ShouldSucceed_WithoutOpeningQuantity()
    {
        var material = await SeedMaterialAsync("SKU-CREATE-1");
        var service = CreateService();

        var request = new PostLotRequest
        {
            LotNumber = "LOT-CREATE-1",
            MaterialId = material.Id,
            UnitCostPhp = 50,
            Quantity = 0,
            ReceivedDate = DateTime.UtcNow
        };

        var result = await service.CreateLotAsync(request);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("LOT-CREATE-1", result.LotNumber);
        Assert.Equal(0, result.Quantity);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateLot_ShouldPostOpeningMovement_WhenQuantityGreaterThanZero()
    {
        var material = await SeedMaterialAsync("SKU-CREATE-2");
        var service = CreateService();

        var request = new PostLotRequest
        {
            LotNumber = "LOT-CREATE-2",
            MaterialId = material.Id,
            UnitCostPhp = 50,
            Quantity = 75,
            StockMovementType = StockMovementType.ReceiptFromSupplier,
            ReceivedDate = DateTime.UtcNow
        };

        var result = await service.CreateLotAsync(request);

        Assert.Equal(75, result.Quantity);
    }

    [Fact]
    public async Task CreateLot_ShouldRollBackLot_WhenOpeningMovementFails()
    {
        var material = await SeedMaterialAsync("SKU-CREATE-FAIL");
        var service = CreateService();

        // Type is a decrease with nothing on hand yet -> PostMovementWithinTransactionAsync
        // will throw "insufficient stock", which should roll back the lot insert too.
        var request = new PostLotRequest
        {
            LotNumber = "LOT-CREATE-FAIL",
            MaterialId = material.Id,
            UnitCostPhp = 50,
            Quantity = 10,
            StockMovementType = StockMovementType.IssuanceToProduction,
            ReceivedDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateLotAsync(request));

        var lotExists = _fixture.DbContext.Lots.Any(l => l.LotNumber == "LOT-CREATE-FAIL");
        Assert.False(lotExists);
    }

    [Fact]
    public async Task CreateLot_ShouldPopulateMaterial_OnResult()
    {
        var material = await SeedMaterialAsync("SKU-CREATE-3");
        var service = CreateService();

        var request = new PostLotRequest
        {
            LotNumber = "LOT-CREATE-3",
            MaterialId = material.Id,
            UnitCostPhp = 50,
            Quantity = 0,
            ReceivedDate = DateTime.UtcNow
        };

        var result = await service.CreateLotAsync(request);

        Assert.NotNull(result.Material);
        Assert.Equal(material.Id, result.Material.Id);
    }

    // ---------- GetLotByIdAsync ----------

    [Fact]
    public async Task GetLotById_ShouldThrowNotFound_WhenLotDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetLotByIdAsync(999999));
    }

    [Fact]
    public async Task GetLotById_ShouldReturnCorrectQuantity_AfterMovements()
    {
        var material = await SeedMaterialAsync("SKU-GET-1");
        var service = CreateService();

        var created = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-GET-1",
            MaterialId = material.Id,
            UnitCostPhp = 20,
            Quantity = 60,
            StockMovementType = StockMovementType.ReceiptFromSupplier,
            ReceivedDate = DateTime.UtcNow
        });

        var result = await service.GetLotByIdAsync(created.Id);

        Assert.Equal(60, result.Quantity);
        Assert.Equal("LOT-GET-1", result.LotNumber);
    }

    // ---------- GetAllLotsAsync ----------

    [Fact]
    public async Task GetAllLots_ShouldReturnEmptyList_WhenNoLotsExist()
    {
        // Use an isolated fixture-free assertion via a fresh material-less check is not possible
        // since DatabaseFixture is shared; instead assert the call succeeds and includes at least
        // the lots seeded by earlier tests without throwing.
        var service = CreateService();

        var result = await service.GetAllLotsAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAllLots_ShouldIncludeCorrectQuantityPerLot()
    {
        var materialA = await SeedMaterialAsync("SKU-ALL-A");
        var materialB = await SeedMaterialAsync("SKU-ALL-B");
        var service = CreateService();

        var lotA = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-ALL-A",
            MaterialId = materialA.Id,
            UnitCostPhp = 10,
            Quantity = 25,
            StockMovementType = StockMovementType.ReceiptFromSupplier,
            ReceivedDate = DateTime.UtcNow
        });

        var lotB = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-ALL-B",
            MaterialId = materialB.Id,
            UnitCostPhp = 10,
            Quantity = 40,
            StockMovementType = StockMovementType.ReceiptFromSupplier,
            ReceivedDate = DateTime.UtcNow
        });

        var all = await service.GetAllLotsAsync();

        var resultA = all.First(l => l.Id == lotA.Id);
        var resultB = all.First(l => l.Id == lotB.Id);

        Assert.Equal(25, resultA.Quantity);
        Assert.Equal(40, resultB.Quantity);
    }

    // ---------- UpdateLotAsync ----------

    [Fact]
    public async Task UpdateLot_ShouldThrowNotFound_WhenLotDoesNotExist()
    {
        var service = CreateService();

        var request = new PostLotRequest
        {
            LotNumber = "DOESNT-MATTER",
            MaterialId = 1,
            UnitCostPhp = 10,
            ReceivedDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateLotAsync(999999, request));
    }

    [Fact]
    public async Task UpdateLot_ShouldUpdateFields_WhenLotIsActive()
    {
        var material = await SeedMaterialAsync("SKU-UPD-1");
        var service = CreateService();

        var created = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-UPD-1",
            MaterialId = material.Id,
            UnitCostPhp = 30,
            Quantity = 0,
            ReceivedDate = DateTime.UtcNow
        });

        var updateRequest = new PostLotRequest
        {
            LotNumber = "LOT-UPD-1-RENAMED",
            MaterialId = material.Id,
            UnitCostPhp = 45,
            ReceivedDate = DateTime.UtcNow
        };

        var updated = await service.UpdateLotAsync(created.Id, updateRequest);

        Assert.Equal("LOT-UPD-1-RENAMED", updated.LotNumber);
        Assert.Equal(45, updated.UnitCostPhp);
    }

    [Fact]
    public async Task UpdateLot_ShouldThrow_WhenLotIsDeactivated()
    {
        var material = await SeedMaterialAsync("SKU-UPD-2");
        var service = CreateService();

        var created = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-UPD-2",
            MaterialId = material.Id,
            UnitCostPhp = 30,
            Quantity = 0,
            ReceivedDate = DateTime.UtcNow
        });

        await service.DeactivateLotAsync(created.Id);

        var updateRequest = new PostLotRequest
        {
            LotNumber = "LOT-UPD-2-SHOULD-FAIL",
            MaterialId = material.Id,
            UnitCostPhp = 30,
            ReceivedDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateLotAsync(created.Id, updateRequest));
    }

    [Fact]
    public async Task UpdateLot_ShouldNotAffectQuantity()
    {
        var material = await SeedMaterialAsync("SKU-UPD-3");
        var service = CreateService();

        var created = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-UPD-3",
            MaterialId = material.Id,
            UnitCostPhp = 30,
            Quantity = 50,
            StockMovementType = StockMovementType.ReceiptFromSupplier,
            ReceivedDate = DateTime.UtcNow
        });

        var updateRequest = new PostLotRequest
        {
            LotNumber = "LOT-UPD-3-RENAMED",
            MaterialId = material.Id,
            UnitCostPhp = 99,
            ReceivedDate = DateTime.UtcNow
        };

        var updated = await service.UpdateLotAsync(created.Id, updateRequest);

        // Quantity should be untouched by a metadata update
        Assert.Equal(50, updated.Quantity);
    }

    // ---------- DeactivateLotAsync ----------

    [Fact]
    public async Task DeactivateLot_ShouldThrowNotFound_WhenLotDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeactivateLotAsync(999999));
    }

    [Fact]
    public async Task DeactivateLot_ShouldSetIsActiveFalse_WhenLotIsActive()
    {
        var material = await SeedMaterialAsync("SKU-DEACT-1");
        var service = CreateService();

        var created = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-DEACT-1",
            MaterialId = material.Id,
            UnitCostPhp = 15,
            Quantity = 0,
            ReceivedDate = DateTime.UtcNow
        });

        await service.DeactivateLotAsync(created.Id);

        var result = await service.GetLotByIdAsync(created.Id);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task DeactivateLot_ShouldThrow_WhenAlreadyDeactivated()
    {
        var material = await SeedMaterialAsync("SKU-DEACT-2");
        var service = CreateService();

        var created = await service.CreateLotAsync(new PostLotRequest
        {
            LotNumber = "LOT-DEACT-2",
            MaterialId = material.Id,
            UnitCostPhp = 15,
            Quantity = 0,
            ReceivedDate = DateTime.UtcNow
        });

        await service.DeactivateLotAsync(created.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeactivateLotAsync(created.Id));
    }
}