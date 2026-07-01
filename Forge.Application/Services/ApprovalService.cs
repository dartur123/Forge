using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forge.Application.Services;

public class ApprovalService : IApprovalService
{
    private readonly ForgeDbContext _context;

    public ApprovalService(ForgeDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApprovalRequirement>> GetRequiredApprovalsAsync(string entityType)
    {
        var retrieveApprovalRequirement = await _context.ApprovalRules
                                                        .Include(ar => ar.RequiredRole)
                                                        .Where(ar => ar.EntityType == entityType && ar.IsActive)
                                                        .OrderBy(ar => ar.SequenceOrder)
                                                        .ToListAsync();

        return retrieveApprovalRequirement.Select(ApprovalRequirement.FromEntity).ToList();
    }

    public async Task<bool> RequiresApprovalAsync(string entityType)
    {
        var approvals = await GetRequiredApprovalsAsync(entityType);
        return approvals.Count > 0;
    }

    public async Task<ApprovalRuleResult> CreateRuleAsync(PostApprovalRuleRequest request)
    {
        var rule = ApprovalRule.Create(request.EntityType, request.RequiredRoleId, request.SequenceOrder);
        _context.ApprovalRules.Add(rule);
        await _context.SaveChangesAsync();
        var createdRule = await _context.ApprovalRules
                                        .Include(ar => ar.RequiredRole)
                                        .FirstOrDefaultAsync(ar => ar.Id == rule.Id);
        return ApprovalRuleResult.FromEntity(createdRule!);
    }

    public async Task DeactivateRuleAsync(int ruleId)
    {
        var rule = await _context.ApprovalRules.FindAsync(ruleId);
        if (rule == null)
        {
            throw new NotFoundException($"Approval rule with ID {ruleId} not found.");
        }

        rule.Deactivate();
        await _context.SaveChangesAsync();
    }

    public async Task<ApprovalRuleResult> GetRuleAsync(int ruleId)
    {
        var rule = await _context.ApprovalRules
                           .Include(ar => ar.RequiredRole)
                           .FirstOrDefaultAsync(ar => ar.Id == ruleId);

        if(rule == null)
        {
            throw new NotFoundException($"Approval rule with ID {ruleId} not found.");
        }

        return ApprovalRuleResult.FromEntity(rule);
    }

    public async Task<ApprovalInstanceResult> StartApprovalAsync(string entityType, int entityId)
    {
        var requiredApprovals = await GetRequiredApprovalsAsync(entityType);
        if(requiredApprovals.Count == 0)
        {
            throw new InvalidOperationException($"No approval rules defined for entity type '{entityType}'.");
        }

        var approvalInstance = ApprovalInstance.Create(entityType, entityId);

        _context.ApprovalInstances.Add(approvalInstance);
        await _context.SaveChangesAsync();

        return ApprovalInstanceResult.FromEntity(approvalInstance, new List<ApprovalDecision>());
    }

    public async Task<ApprovalInstanceResult> ApproveStepAsync(int instanceId, int userId, string? comment)
    {
        var approvalInstance = await _context.ApprovalInstances.FirstOrDefaultAsync(ai => ai.Id == instanceId);
        if(approvalInstance == null)
            throw new NotFoundException($"Approval instance with ID {instanceId} not found.");

        if (approvalInstance.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve — instance is already {approvalInstance.Status}.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var approvalDecision = ApprovalDecision.Create(approvalInstance.Id, approvalInstance.CurrentSequenceOrder, userId, DecisionType.Approved, comment);
            _context.ApprovalDecisions.Add(approvalDecision);
            await _context.SaveChangesAsync();

            var requiredApprovals = await GetRequiredApprovalsAsync(approvalInstance.EntityType);
            if (requiredApprovals.Count==approvalInstance.CurrentSequenceOrder)
            {
                approvalInstance.Approve();
            }
            else
            {
                approvalInstance.AdvanceToNextStep();
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var updatedApprovalInstances = await _context.ApprovalInstances.Include(ai => ai.Decisions)
                                                                           .FirstOrDefaultAsync(ai => ai.Id == instanceId);

            return ApprovalInstanceResult.FromEntity(updatedApprovalInstances!, updatedApprovalInstances!.Decisions);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApprovalInstanceResult> RejectStepAsync(int instanceId, int userId, string comment)
    {
        var approvalInstance = await _context.ApprovalInstances.FirstOrDefaultAsync(ai => ai.Id == instanceId);
        if (approvalInstance == null)
            throw new NotFoundException($"Approval instance with ID {instanceId} not found.");

        if (approvalInstance.Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject — instance is already {approvalInstance.Status}.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var approvalDecision = ApprovalDecision.Create(approvalInstance.Id, approvalInstance.CurrentSequenceOrder, userId, DecisionType.Rejected, comment);
            _context.ApprovalDecisions.Add(approvalDecision);
            await _context.SaveChangesAsync();

            approvalInstance.Reject();
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var updatedApprovalInstances = await _context.ApprovalInstances.Include(ai => ai.Decisions)
                                                                           .FirstOrDefaultAsync(ai => ai.Id == instanceId);

            return ApprovalInstanceResult.FromEntity(updatedApprovalInstances!, updatedApprovalInstances!.Decisions);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApprovalInstanceResult> ResubmitAsync(int instanceId)
    {
        var approvalInstance = await _context.ApprovalInstances.FirstOrDefaultAsync(ai => ai.Id == instanceId);
        if (approvalInstance == null)
            throw new NotFoundException($"Approval instance with ID {instanceId} not found.");

        if (approvalInstance.Status != ApprovalStatus.Rejected)
            throw new InvalidOperationException($"Only rejected approval can be re-submitted.");

        approvalInstance.Resubmit();
        await _context.SaveChangesAsync();

        var updatedApprovalInstances = await _context.ApprovalInstances.Include(ai => ai.Decisions)
                                                                       .FirstOrDefaultAsync(ai => ai.Id == instanceId);
        
        return ApprovalInstanceResult.FromEntity(updatedApprovalInstances!, updatedApprovalInstances!.Decisions);
    }

    public async Task<ApprovalInstanceResult> GetInstanceAsync(int instanceId)
    {
        var approvalInstance = await _context.ApprovalInstances.Include(ai => ai.Decisions)
                                                               .FirstOrDefaultAsync(ai => ai.Id == instanceId);

        if (approvalInstance == null)
            throw new NotFoundException($"Approval instance with ID {instanceId} not found.");

        return ApprovalInstanceResult.FromEntity(approvalInstance!, approvalInstance!.Decisions);
    }
}
