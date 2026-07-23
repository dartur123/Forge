using Forge.Domain;
using Forge.Domain.Enums;

namespace Forge.Application.Responses;

public class ApprovalInstanceResult
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public ApprovalStatus Status { get; set; }
    public int CurrentSequenceOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ApprovalDecisionResult> Decisions { get; set; } = new();

    public static ApprovalInstanceResult FromEntity(ApprovalInstance instance, List<ApprovalDecision> decisions)
    {
        return new ApprovalInstanceResult
        {
            Id = instance.Id,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            Status = instance.Status,
            CurrentSequenceOrder = instance.CurrentSequenceOrder,
            CreatedAt = instance.CreatedAt,
            Decisions = decisions.Select(ApprovalDecisionResult.FromEntity).ToList()
        };
    }
}