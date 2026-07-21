using Forge.Application.Requests;
using Forge.Application.Services;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Tests;

public class PurchaseOrderServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PurchaseOrderServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private PurchaseOrderService CreateService()
    {
        return new PurchaseOrderService(_fixture.DbContext);
    }

    private async Task<Supplier> SeedSupplierAsync(string name)
    {
        var supplier = Supplier.Create(name, "USD", null, null, null);
        _fixture.DbContext.Suppliers.Add(supplier);
        await _fixture.DbContext.SaveChangesAsync();
        return supplier;
    }

    private async Task<Material> SeedMaterialAsync(string sku)
    {
        var material = Material.Create(sku, $"{sku} Material", MaterialType.Raw, null, "pcs");
        _fixture.DbContext.Materials.Add(material);
        await _fixture.DbContext.SaveChangesAsync();
        return material;
    }

    // ---------- CreateAsync ----------

    [Fact]
    public async Task Create_ShouldSucceed_WithoutLines()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-1");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-CREATE-1",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1.5m,
            CreatedByUserId = 1
        };

        var result = await service.CreateAsync(request);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("PO-CREATE-1", result.OrderNumber);
        Assert.Equal(supplier.Id, result.SupplierId);
        Assert.Equal(PurchaseOrderStatus.Draft, result.Status);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(1.5m, result.ExchangeRate);
        Assert.Equal(1, result.CreatedByUserId);
        Assert.False(result.IsSentToSupplier);
        Assert.Empty(result.Lines);
        Assert.Equal(0, result.TotalCostForeign);
    }

    [Fact]
    public async Task Create_ShouldPersistLines_WhenLinesProvided()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-2");
        var materialA = await SeedMaterialAsync("SKU-PO-CREATE-A");
        var materialB = await SeedMaterialAsync("SKU-PO-CREATE-B");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-CREATE-2",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = materialA.Id, Quantity = 10, UnitCostForeign = 5 },
                new() { MaterialId = materialB.Id, Quantity = 3, UnitCostForeign = 20 }
            }
        };

        var result = await service.CreateAsync(request);

        Assert.Equal(2, result.Lines.Count);
        Assert.Contains(result.Lines, l => l.MaterialId == materialA.Id && l.Quantity == 10 && l.UnitCostForeign == 5);
        Assert.Contains(result.Lines, l => l.MaterialId == materialB.Id && l.Quantity == 3 && l.UnitCostForeign == 20);
    }

    [Fact]
    public async Task Create_ShouldComputeTotalCostForeign_AsSumOfLineTotals()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-3");
        var materialA = await SeedMaterialAsync("SKU-PO-CREATE-C");
        var materialB = await SeedMaterialAsync("SKU-PO-CREATE-D");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-CREATE-3",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = materialA.Id, Quantity = 10, UnitCostForeign = 5 },   // 50
                new() { MaterialId = materialB.Id, Quantity = 3, UnitCostForeign = 20 }    // 60
            }
        };

        var result = await service.CreateAsync(request);

        Assert.Equal(110, result.TotalCostForeign);
    }

    [Fact]
    public async Task Create_ShouldThrow_WhenOrderNumberIsEmpty()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-4");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1
        };

        await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task Create_ShouldThrow_WhenLineQuantityIsNotPositive()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-5");
        var material = await SeedMaterialAsync("SKU-PO-CREATE-E");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-CREATE-5",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = 0, UnitCostForeign = 5 }
            }
        };

        await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task Create_ShouldNotPersistOrder_WhenLineCreationFails()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-6");
        var material = await SeedMaterialAsync("SKU-PO-CREATE-F");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-CREATE-6-SHOULD-NOT-EXIST",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = -1, UnitCostForeign = 5 }
            }
        };

        await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(request));

        var exists = _fixture.DbContext.PurchaseOrders.Any(po => po.OrderNumber == "PO-CREATE-6-SHOULD-NOT-EXIST");
        Assert.False(exists);
    }

    [Fact]
    public async Task Create_ShouldDefaultCurrencyAndExchangeRate_WhenNotSpecified()
    {
        var supplier = await SeedSupplierAsync("Supplier-Create-7");
        var service = CreateService();

        var request = new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-CREATE-7",
            SupplierId = supplier.Id,
            CreatedByUserId = 1
        };

        var result = await service.CreateAsync(request);

        Assert.Equal("PHP", result.Currency);
        Assert.Equal(1.0m, result.ExchangeRate);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetById_ShouldThrowKeyNotFound_WhenPurchaseOrderDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(999999));
    }

    [Fact]
    public async Task GetById_ShouldReturnPurchaseOrder_WithLines()
    {
        var supplier = await SeedSupplierAsync("Supplier-Get-1");
        var material = await SeedMaterialAsync("SKU-PO-GET-A");
        var service = CreateService();

        var created = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-GET-1",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = 4, UnitCostForeign = 25 }
            }
        });

        var result = await service.GetByIdAsync(created.Id);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal("PO-GET-1", result.OrderNumber);
        Assert.Single(result.Lines);
        Assert.Equal(100, result.TotalCostForeign);
    }

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAll_ShouldNotIncludeLines_WhenIncludeLinesIsFalse()
    {
        var supplier = await SeedSupplierAsync("Supplier-GetAll-1");
        var material = await SeedMaterialAsync("SKU-PO-GETALL-A");
        var service = CreateService();

        var created = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-GETALL-1",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = 1, UnitCostForeign = 10 }
            }
        });

        _fixture.DbContext.ChangeTracker.Clear();

        var all = await service.GetAllAsync(includeLines: false);

        var result = all.First(po => po.Id == created.Id);
        Assert.Empty(result.Lines);
    }

    [Fact]
    public async Task GetAll_ShouldIncludeLines_WhenIncludeLinesIsTrue()
    {
        var supplier = await SeedSupplierAsync("Supplier-GetAll-2");
        var material = await SeedMaterialAsync("SKU-PO-GETALL-B");
        var service = CreateService();

        var created = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-GETALL-2",
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = 2, UnitCostForeign = 15 }
            }
        });

        var all = await service.GetAllAsync(includeLines: true);

        var result = all.First(po => po.Id == created.Id);
        Assert.Single(result.Lines);
        Assert.Equal(30, result.TotalCostForeign);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllCreatedPurchaseOrders()
    {
        var supplier = await SeedSupplierAsync("Supplier-GetAll-3");
        var service = CreateService();

        var createdA = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-GETALL-3-A",
            SupplierId = supplier.Id,
            CreatedByUserId = 1
        });
        var createdB = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-GETALL-3-B",
            SupplierId = supplier.Id,
            CreatedByUserId = 1
        });

        var all = await service.GetAllAsync(includeLines: false);

        Assert.Contains(all, po => po.Id == createdA.Id);
        Assert.Contains(all, po => po.Id == createdB.Id);
    }
}
