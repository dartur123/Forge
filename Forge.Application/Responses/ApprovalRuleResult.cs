using Forge.Domain;

namespace Forge.Application.Responses
{
    public class ApprovalRuleResult
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public RoleResult RequiredRole { get; set; } = null!;
        public int SequenceOrder { get; set; }
        public bool IsActive { get; set; }

        public static ApprovalRuleResult FromEntity(ApprovalRule approvalRule) => new()
        {
            Id = approvalRule.Id,
            EntityType = approvalRule.EntityType,
            IsActive = approvalRule.IsActive,
            RequiredRole = RoleResult.FromEntity(approvalRule.RequiredRole),
            SequenceOrder = approvalRule.SequenceOrder
        };
    }
}
