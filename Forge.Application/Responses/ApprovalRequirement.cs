using Forge.Domain;

namespace Forge.Application.Responses;

public class ApprovalRequirement
{
    public int SequenceOrder { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public static ApprovalRequirement FromEntity(ApprovalRule approvalRule) => new()
    {
        RoleId = approvalRule.RequiredRoleId,
        RoleName = approvalRule.RequiredRole?.Name ?? string.Empty,
        SequenceOrder = approvalRule.SequenceOrder
    };
}