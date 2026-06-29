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
        var lot = Lot.Create("LOT-001", material.Id, null, null, 50, 100, DateTime.UtcNow, null);
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
        var lot = Lot.Create("LOT-002", material.Id, null, null, 100, 50, DateTime.UtcNow, null);
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

    [Fact]
    public async Task PostMovement_ShouldFail_WhenQuantityIsZeroOrBelow()
    {
        // Arrange
        var service = new StockLedgerService(_fixture.DbContext);

        var request = new PostStockMovementRequest
        {
            LotId = 1,
            Quantity = 0,
            Type = StockMovementType.IssuanceToProduction,
            TransactionDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PostMovementAsync(request));
    }
}