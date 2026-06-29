using Forge.Application.Requests;
using Forge.Application.Services;
using Forge.Domain;
using Forge.Domain.Enums;
using Xunit;

namespace Forge.Tests;

public class StockLedgerServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public StockLedgerServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PostMovement_ShouldFail_WhenIssuanceExceedsLotQuantity()
    {
        // Arrange
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = 999,
            Quantity = 9999,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));
    }

    [Fact]
    public async Task PostMovement_ShouldFail_WhenIssuanceExceedsAvailableStock()
    {
        // Arrange
        var material = Material.Create("SKU-001", "Test Material", MaterialType.Raw, null, "pcs");
        _fixture.DbContext.Materials.Add(material);
        await _fixture.DbContext.SaveChangesAsync();

        var lot = new Lot
        {
            LotNumber = "LOT-001",
            MaterialId = material.Id,
            Quantity = 50,
            UnitCostPhp = 100,
            IsActive = true
        };
        _fixture.DbContext.Lots.Add(lot);
        await _fixture.DbContext.SaveChangesAsync();

        var role = new Role { Name = "Warehouse Staff" };
        _fixture.DbContext.Roles.Add(role);
        await _fixture.DbContext.SaveChangesAsync();

        var user = new User
        {
            Name = "Test User",
            Email = "test@forge.com",
            PasswordHash = "placeholder",
            RoleId = role.Id,
            IsActive = true
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 100,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow,
            ReleasedByUserId = user.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));
    }

    [Fact]
    public async Task PostMovement_ShouldDeductLotQuantity_WhenIssuanceIsValid()
    {
        // Arrange
        var material = Material.Create("SKU-002", "Test Material 2", MaterialType.Raw, null, "pcs");
        _fixture.DbContext.Materials.Add(material);
        await _fixture.DbContext.SaveChangesAsync();

        var lot = new Lot
        {
            LotNumber = "LOT-002",
            MaterialId = material.Id,
            Quantity = 100,
            UnitCostPhp = 50,
            IsActive = true
        };
        _fixture.DbContext.Lots.Add(lot);
        await _fixture.DbContext.SaveChangesAsync();

        var role = new Role { Name = "Warehouse Staff" };
        _fixture.DbContext.Roles.Add(role);
        await _fixture.DbContext.SaveChangesAsync();

        var user = new User
        {
            Name = "Test User",
            Email = "test@forge.com",
            PasswordHash = "placeholder",
            RoleId = role.Id,
            IsActive = true
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = lot.Id,
            Quantity = 30,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow,
            ReleasedByUserId = user.Id
        };

        // Act
        var result = await service.PostMovementAsync(request);

        // Assert
        Assert.Equal(70, result.RemainingLotQuantity);
    }
}