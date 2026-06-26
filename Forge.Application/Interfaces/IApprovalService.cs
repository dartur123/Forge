using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain;

namespace Forge.Application.Interfaces;

public interface IApprovalService
{
    Task<List<ApprovalRequirement>> GetRequiredApprovalsAsync(string entityType);
    Task<bool> RequiresApprovalAsync(string entityType);
    Task<ApprovalRuleResult> GetRuleAsync(int ruleId);
    Task<ApprovalRuleResult> CreateRuleAsync(PostApprovalRuleRequest request);
    Task DeactivateRuleAsync(int ruleId);
}