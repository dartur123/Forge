using Forge.Application.Requests;
using Forge.Application.Responses;

namespace Forge.Application.Interfaces;

public interface IApprovalService
{
    Task<List<ApprovalRequirement>> GetRequiredApprovalsAsync(string entityType);
    Task<bool> RequiresApprovalAsync(string entityType);
    Task<ApprovalRuleResult> GetRuleAsync(int ruleId);
    Task<ApprovalRuleResult> CreateRuleAsync(PostApprovalRuleRequest request);
    Task DeactivateRuleAsync(int ruleId);
    Task<ApprovalInstanceResult> StartApprovalAsync(string entityType, int entityId);
    Task<ApprovalInstanceResult> ApproveStepAsync(int instanceId, int userId, string? comment);
    Task<ApprovalInstanceResult> RejectStepAsync(int instanceId, int userId, string comment);
    Task<ApprovalInstanceResult> ResubmitAsync(int instanceId);
    Task<ApprovalInstanceResult> GetInstanceAsync(int instanceId);
}