using Forge.Application.Exceptions;
using Forge.Application.Requests;
using Forge.Application.Services;
using Forge.Domain;
using Forge.Domain.Exceptions;
using Xunit;

namespace Forge.Tests;

public class ApprovalServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ApprovalServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private ApprovalService CreateService() => new ApprovalService(_fixture.DbContext);

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

    private async Task SeedRuleAsync(string entityType, int roleId, int sequenceOrder)
    {
        var rule = ApprovalRule.Create(entityType, roleId, sequenceOrder);
        _fixture.DbContext.ApprovalRules.Add(rule);
        await _fixture.DbContext.SaveChangesAsync();
    }

    // ---------- Rule management ----------

    [Fact]
    public async Task CreateRule_ShouldSucceed_WithValidRequest()
    {
        var role = await SeedRoleAsync("Supervisor-Create");
        var service = CreateService();

        var request = new PostApprovalRuleRequest
        {
            EntityType = "TestEntity-Create",
            RequiredRoleId = role.Id,
            SequenceOrder = 1
        };

        var result = await service.CreateRuleAsync(request);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("TestEntity-Create", result.EntityType);
    }

    [Fact]
    public async Task GetRule_ShouldThrowNotFound_WhenRuleDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetRuleAsync(999999));
    }

    [Fact]
    public async Task DeactivateRule_ShouldThrowNotFound_WhenRuleDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeactivateRuleAsync(999999));
    }

    [Fact]
    public async Task DeactivateRule_ShouldExcludeFromRequiredApprovals()
    {
        var role = await SeedRoleAsync("Supervisor-Deactivate");
        var service = CreateService();

        var rule = await service.CreateRuleAsync(new PostApprovalRuleRequest
        {
            EntityType = "TestEntity-Deactivate",
            RequiredRoleId = role.Id,
            SequenceOrder = 1
        });

        await service.DeactivateRuleAsync(rule.Id);

        var required = await service.GetRequiredApprovalsAsync("TestEntity-Deactivate");
        Assert.Empty(required);
    }

    [Fact]
    public async Task RequiresApproval_ShouldReturnFalse_WhenNoRulesExist()
    {
        var service = CreateService();

        var result = await service.RequiresApprovalAsync("NonExistentEntityType");

        Assert.False(result);
    }

    [Fact]
    public async Task RequiresApproval_ShouldReturnTrue_WhenRulesExist()
    {
        var role = await SeedRoleAsync("Supervisor-Requires");
        var service = CreateService();

        await service.CreateRuleAsync(new PostApprovalRuleRequest
        {
            EntityType = "TestEntity-Requires",
            RequiredRoleId = role.Id,
            SequenceOrder = 1
        });

        var result = await service.RequiresApprovalAsync("TestEntity-Requires");

        Assert.True(result);
    }

    // ---------- StartApprovalAsync ----------

    [Fact]
    public async Task StartApproval_ShouldFail_WhenNoRulesConfigured()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartApprovalAsync("EntityWithNoRules", 1));
    }

    [Fact]
    public async Task StartApproval_ShouldCreateInstance_WithStatusPendingAtStepOne()
    {
        var role = await SeedRoleAsync("Supervisor-Start");
        await SeedRuleAsync("TestEntity-Start", role.Id, 1);
        var service = CreateService();

        var instance = await service.StartApprovalAsync("TestEntity-Start", 42);

        Assert.Equal("Pending", instance.Status);
        Assert.Equal(1, instance.CurrentSequenceOrder);
        Assert.Equal("TestEntity-Start", instance.EntityType);
        Assert.Equal(42, instance.EntityId);
        Assert.Empty(instance.Decisions);
    }

    // ---------- ApproveStepAsync ----------

    [Fact]
    public async Task ApproveStep_ShouldThrowNotFound_WhenInstanceDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ApproveStepAsync(999999, 1, null));
    }

    [Fact]
    public async Task ApproveStep_ShouldAdvanceToNextStep_WhenNotLastStep()
    {
        var supervisorRole = await SeedRoleAsync("Supervisor-Advance");
        var managerRole = await SeedRoleAsync("Manager-Advance");
        var supervisor = await SeedUserAsync("supervisor-advance@forge.com", supervisorRole.Id);

        await SeedRuleAsync("TestEntity-Advance", supervisorRole.Id, 1);
        await SeedRuleAsync("TestEntity-Advance", managerRole.Id, 2);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-Advance", 1);

        var updated = await service.ApproveStepAsync(instance.Id, supervisor.Id, "Looks good");

        Assert.Equal("Pending", updated.Status);
        Assert.Equal(2, updated.CurrentSequenceOrder);
        Assert.Single(updated.Decisions);
        Assert.Equal("Approved", updated.Decisions[0].Decision);
    }

    [Fact]
    public async Task ApproveStep_ShouldMarkApproved_WhenLastStep()
    {
        var role = await SeedRoleAsync("Supervisor-LastStep");
        var user = await SeedUserAsync("supervisor-laststep@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-LastStep", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-LastStep", 1);

        var updated = await service.ApproveStepAsync(instance.Id, user.Id, null);

        Assert.Equal("Approved", updated.Status);
    }

    [Fact]
    public async Task ApproveStep_ShouldFail_WhenInstanceAlreadyApproved()
    {
        var role = await SeedRoleAsync("Supervisor-AlreadyApproved");
        var user = await SeedUserAsync("supervisor-alreadyapproved@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-AlreadyApproved", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-AlreadyApproved", 1);
        await service.ApproveStepAsync(instance.Id, user.Id, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApproveStepAsync(instance.Id, user.Id, null));
    }

    [Fact]
    public async Task ApproveStep_MultiStep_ShouldReachApproved_AfterAllStepsSigned()
    {
        var supervisorRole = await SeedRoleAsync("Supervisor-MultiStep");
        var managerRole = await SeedRoleAsync("Manager-MultiStep");
        var supervisor = await SeedUserAsync("supervisor-multistep@forge.com", supervisorRole.Id);
        var manager = await SeedUserAsync("manager-multistep@forge.com", managerRole.Id);

        await SeedRuleAsync("TestEntity-MultiStep", supervisorRole.Id, 1);
        await SeedRuleAsync("TestEntity-MultiStep", managerRole.Id, 2);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-MultiStep", 1);

        await service.ApproveStepAsync(instance.Id, supervisor.Id, "Step 1 ok");
        var final = await service.ApproveStepAsync(instance.Id, manager.Id, "Step 2 ok");

        Assert.Equal("Approved", final.Status);
        Assert.Equal(2, final.Decisions.Count);
    }

    // ---------- RejectStepAsync ----------

    [Fact]
    public async Task RejectStep_ShouldThrowNotFound_WhenInstanceDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.RejectStepAsync(999999, 1, "not valid"));
    }

    [Fact]
    public async Task RejectStep_ShouldMarkInstanceRejected()
    {
        var role = await SeedRoleAsync("Supervisor-Reject");
        var user = await SeedUserAsync("supervisor-reject@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-Reject", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-Reject", 1);

        var updated = await service.RejectStepAsync(instance.Id, user.Id, "Missing documents");

        Assert.Equal("Rejected", updated.Status);
        Assert.Single(updated.Decisions);
        Assert.Equal("Rejected", updated.Decisions[0].Decision);
        Assert.Equal("Missing documents", updated.Decisions[0].Comment);
    }

    [Fact]
    public async Task RejectStep_ShouldFail_WithoutComment()
    {
        var role = await SeedRoleAsync("Supervisor-NoComment");
        var user = await SeedUserAsync("supervisor-nocomment@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-NoComment", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-NoComment", 1);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.RejectStepAsync(instance.Id, user.Id, ""));
    }

    [Fact]
    public async Task RejectStep_ShouldFail_WhenInstanceAlreadyRejected()
    {
        var role = await SeedRoleAsync("Supervisor-AlreadyRejected");
        var user = await SeedUserAsync("supervisor-alreadyrejected@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-AlreadyRejected", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-AlreadyRejected", 1);
        await service.RejectStepAsync(instance.Id, user.Id, "First rejection");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RejectStepAsync(instance.Id, user.Id, "Second rejection"));
    }

    // ---------- ResubmitAsync ----------

    [Fact]
    public async Task Resubmit_ShouldThrowNotFound_WhenInstanceDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ResubmitAsync(999999));
    }

    [Fact]
    public async Task Resubmit_ShouldFail_WhenInstanceIsNotRejected()
    {
        var role = await SeedRoleAsync("Supervisor-ResubmitPending");
        await SeedRuleAsync("TestEntity-ResubmitPending", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-ResubmitPending", 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResubmitAsync(instance.Id));
    }

    [Fact]
    public async Task Resubmit_ShouldResetToStepOne_AndStatusPending()
    {
        var role = await SeedRoleAsync("Supervisor-ResubmitReset");
        var user = await SeedUserAsync("supervisor-resubmitreset@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-ResubmitReset", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-ResubmitReset", 1);
        await service.RejectStepAsync(instance.Id, user.Id, "Needs changes");

        var resubmitted = await service.ResubmitAsync(instance.Id);

        Assert.Equal("Pending", resubmitted.Status);
        Assert.Equal(1, resubmitted.CurrentSequenceOrder);
    }

    [Fact]
    public async Task Resubmit_ShouldPreserveDecisionHistory_AfterRejectResubmitApprove()
    {
        var role = await SeedRoleAsync("Supervisor-FullCycle");
        var user = await SeedUserAsync("supervisor-fullcycle@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-FullCycle", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-FullCycle", 1);

        await service.RejectStepAsync(instance.Id, user.Id, "First round rejection");
        await service.ResubmitAsync(instance.Id);
        var final = await service.ApproveStepAsync(instance.Id, user.Id, "Approved on second round");

        Assert.Equal("Approved", final.Status);
        Assert.Equal(2, final.Decisions.Count);
        Assert.Contains(final.Decisions, d => d.Decision == "Rejected");
        Assert.Contains(final.Decisions, d => d.Decision == "Approved");
    }

    // ---------- GetInstanceAsync ----------

    [Fact]
    public async Task GetInstance_ShouldThrowNotFound_WhenInstanceDoesNotExist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetInstanceAsync(999999));
    }

    [Fact]
    public async Task GetInstance_ShouldReturnCurrentStateWithDecisions()
    {
        var role = await SeedRoleAsync("Supervisor-GetInstance");
        var user = await SeedUserAsync("supervisor-getinstance@forge.com", role.Id);

        await SeedRuleAsync("TestEntity-GetInstance", role.Id, 1);

        var service = CreateService();
        var instance = await service.StartApprovalAsync("TestEntity-GetInstance", 1);
        await service.ApproveStepAsync(instance.Id, user.Id, "Confirmed");

        var fetched = await service.GetInstanceAsync(instance.Id);

        Assert.Equal("Approved", fetched.Status);
        Assert.Single(fetched.Decisions);
    }
}