using Forge.Domain;

namespace Forge.Application.Responses;

public class ApprovalDecisionResult
{
    public int Id { get; set; }
    public int SequenceOrder { get; set; }
    public int DecidedByUserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime DecidedAt { get; set; }

    public static ApprovalDecisionResult FromEntity(ApprovalDecision decision)
    {
        return new ApprovalDecisionResult
        {
            Id = decision.Id,
            SequenceOrder = decision.SequenceOrder,
            DecidedByUserId = decision.DecidedByUserId,
            Decision = decision.Decision.ToString(),
            Comment = decision.Comment,
            DecidedAt = decision.DecidedAt
        };
    }
}