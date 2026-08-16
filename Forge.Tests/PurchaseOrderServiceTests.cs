using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Application.Services;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

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
        return new PurchaseOrderService(_fixture.DbContext, new ApprovalService(_fixture.DbContext));
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

    private async Task<Role> SeedRoleAsync(string name)
    {
        var role = new Role { Name = name };
        _fixture.DbContext.Roles.Add(role);
        await _fixture.DbContext.SaveChangesAsync();
        return role;
    }

    private async Task<User> SeedUserAsync(string email, int roleId)
    {
        var user = new User
        {
            Name = "Test User",
            Email = email,
            PasswordHash = "placeholder",
            RoleId = roleId,
            IsActive = true
        };
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();
        return user;
    }

    // All purchase orders share the single hard-coded entity type "PurchaseOrder" (see
    // PurchaseOrderService.ApproveAsync/RejectAsync/SubmitAsync), so the approval rule for it must
    // be seeded exactly once per test run and reused, rather than seeded fresh per test like
    // ApprovalServiceTests does with its per-test entity type names.
    private async Task<Role> EnsurePurchaseOrderApprovalRoleAsync()
    {
        var existingRule = await _fixture.DbContext.ApprovalRules
            .FirstOrDefaultAsync(r => r.EntityType == nameof(PurchaseOrder) && r.IsActive);

        if (existingRule != null)
        {
            return await _fixture.DbContext.Roles.FirstAsync(r => r.Id == existingRule.RequiredRoleId);
        }

        var role = await SeedRoleAsync("PO-Approver");
        var approvalService = new ApprovalService(_fixture.DbContext);
        await approvalService.CreateRuleAsync(new PostApprovalRuleRequest
        {
            EntityType = nameof(PurchaseOrder),
            RequiredRoleId = role.Id,
            SequenceOrder = 1
        });

        return role;
    }

    private async Task<PurchaseOrderResult> CreateAndSubmitPurchaseOrderAsync(string orderNumber, IPurchaseOrderService service)
    {
        var supplier = await SeedSupplierAsync($"Supplier-{orderNumber}");
        var material = await SeedMaterialAsync($"SKU-{orderNumber}");

        var created = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = orderNumber,
            SupplierId = supplier.Id,
            Currency = "USD",
            ExchangeRate = 1,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = 1, UnitCostForeign = 10 }
            }
        });

        await service.SubmitAsync(created.Id);

        return await service.GetByIdAsync(created.Id);
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

    // ---------- SubmitAsync ----------

    [Fact]
    public async Task Submit_ShouldThrowNotFound_WhenPurchaseOrderDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.SubmitAsync(999999));
    }

    [Fact]
    public async Task Submit_ShouldThrow_WhenPurchaseOrderIsNotDraft()
    {
        await EnsurePurchaseOrderApprovalRoleAsync();
        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-SUBMIT-TWICE", service);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SubmitAsync(submitted.Id));
    }

    [Fact]
    public async Task Submit_ShouldTransitionToSubmitted_AndCreateApprovalInstance()
    {
        await EnsurePurchaseOrderApprovalRoleAsync();
        var service = CreateService();

        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-SUBMIT-OK", service);

        Assert.Equal(PurchaseOrderStatus.Submitted, submitted.Status);

        var approvalService = new ApprovalService(_fixture.DbContext);
        var instance = await approvalService.GetInstanceAsyncByEntityNameId(nameof(PurchaseOrder), submitted.Id);
        Assert.Equal(ApprovalStatus.Pending, instance.Status);
    }

    // ---------- ApproveAsync ----------

    [Fact]
    public async Task Approve_ShouldThrowNotFound_WhenPurchaseOrderDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.ApproveAsync(999999, 1, "ok"));
    }

    [Fact]
    public async Task Approve_ShouldThrow_WhenPurchaseOrderIsNotSubmitted()
    {
        var supplier = await SeedSupplierAsync("Supplier-Approve-NotSubmitted");
        var service = CreateService();

        var draft = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-APPROVE-DRAFT",
            SupplierId = supplier.Id,
            CreatedByUserId = 1
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveAsync(draft.Id, 1, null));
    }

    [Fact]
    public async Task Approve_ShouldThrowForbidden_WhenUserLacksRequiredRole()
    {
        await EnsurePurchaseOrderApprovalRoleAsync();
        var wrongRole = await SeedRoleAsync("PO-Approve-WrongRole");
        var wrongUser = await SeedUserAsync("po-approve-wrongrole@forge.com", wrongRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-APPROVE-FORBIDDEN", service);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.ApproveAsync(submitted.Id, wrongUser.Id, null));

        var stillSubmitted = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Submitted, stillSubmitted.Status);
    }

    [Fact]
    public async Task Approve_ShouldMarkPurchaseOrderApproved_WhenApprovalStepCompletes()
    {
        var approverRole = await EnsurePurchaseOrderApprovalRoleAsync();
        var approver = await SeedUserAsync("po-approve-success@forge.com", approverRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-APPROVE-OK", service);

        await service.ApproveAsync(submitted.Id, approver.Id, "Looks good");

        var approved = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Approved, approved.Status);
    }

    [Fact]
    public async Task Approve_ShouldThrowNotFound_WhenNoApprovalInstanceExists()
    {
        // A purchase order that reached Submitted without going through SubmitAsync (e.g. data
        // seeded directly) has no approval instance behind it — that must surface as a lookup
        // failure rather than silently approving.
        var supplier = await SeedSupplierAsync("Supplier-Approve-NoInstance");
        var material = await SeedMaterialAsync("SKU-Approve-NoInstance");
        var service = CreateService();

        var created = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-APPROVE-NOINSTANCE",
            SupplierId = supplier.Id,
            CreatedByUserId = 1,
            Lines = new List<PostPurchaseOrderLineRequest>
            {
                new() { MaterialId = material.Id, Quantity = 1, UnitCostForeign = 10 }
            }
        });

        var purchaseOrder = await _fixture.DbContext.PurchaseOrders.FirstAsync(po => po.Id == created.Id);
        purchaseOrder.Submit();
        await _fixture.DbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => service.ApproveAsync(created.Id, 1, null));
    }

    [Fact]
    public async Task Approve_ShouldThrow_WhenPurchaseOrderIsAlreadyApproved()
    {
        var approverRole = await EnsurePurchaseOrderApprovalRoleAsync();
        var approver = await SeedUserAsync("po-approve-already@forge.com", approverRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-APPROVE-ALREADY", service);

        await service.ApproveAsync(submitted.Id, approver.Id, "First approval");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveAsync(submitted.Id, approver.Id, "Second approval"));

        var stillApproved = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Approved, stillApproved.Status);
    }

    // ---------- RejectAsync ----------

    [Fact]
    public async Task Reject_ShouldThrowNotFound_WhenPurchaseOrderDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.RejectAsync(999999, 1, "not valid"));
    }

    [Fact]
    public async Task Reject_ShouldThrow_WhenPurchaseOrderIsNotSubmitted()
    {
        var supplier = await SeedSupplierAsync("Supplier-Reject-NotSubmitted");
        var service = CreateService();

        var draft = await service.CreateAsync(new PostPurchaseOrderRequest
        {
            OrderNumber = "PO-REJECT-DRAFT",
            SupplierId = supplier.Id,
            CreatedByUserId = 1
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RejectAsync(draft.Id, 1, "no"));
    }

    [Fact]
    public async Task Reject_ShouldThrowForbidden_WhenUserLacksRequiredRole()
    {
        await EnsurePurchaseOrderApprovalRoleAsync();
        var wrongRole = await SeedRoleAsync("PO-Reject-WrongRole");
        var wrongUser = await SeedUserAsync("po-reject-wrongrole@forge.com", wrongRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-REJECT-FORBIDDEN", service);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.RejectAsync(submitted.Id, wrongUser.Id, "Not allowed"));

        var stillSubmitted = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Submitted, stillSubmitted.Status);
    }

    [Fact]
    public async Task Reject_ShouldThrowDomainException_WhenCommentMissing()
    {
        var approverRole = await EnsurePurchaseOrderApprovalRoleAsync();
        var approver = await SeedUserAsync("po-reject-nocomment@forge.com", approverRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-REJECT-NOCOMMENT", service);

        await Assert.ThrowsAsync<DomainException>(() => service.RejectAsync(submitted.Id, approver.Id, null));

        var stillSubmitted = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Submitted, stillSubmitted.Status);
    }

    [Fact]
    public async Task Reject_ShouldMarkPurchaseOrderRejected_WhenApprovalStepRejected()
    {
        var approverRole = await EnsurePurchaseOrderApprovalRoleAsync();
        var approver = await SeedUserAsync("po-reject-success@forge.com", approverRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-REJECT-OK", service);

        await service.RejectAsync(submitted.Id, approver.Id, "Missing budget approval");

        var rejected = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Rejected, rejected.Status);
    }

    [Fact]
    public async Task Reject_ShouldThrow_WhenPurchaseOrderIsAlreadyRejected()
    {
        var approverRole = await EnsurePurchaseOrderApprovalRoleAsync();
        var approver = await SeedUserAsync("po-reject-already@forge.com", approverRole.Id);

        var service = CreateService();
        var submitted = await CreateAndSubmitPurchaseOrderAsync("PO-REJECT-ALREADY", service);

        await service.RejectAsync(submitted.Id, approver.Id, "First rejection");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RejectAsync(submitted.Id, approver.Id, "Second rejection"));

        var stillRejected = await service.GetByIdAsync(submitted.Id);
        Assert.Equal(PurchaseOrderStatus.Rejected, stillRejected.Status);
    }
}
