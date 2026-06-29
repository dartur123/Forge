using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;
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
}
