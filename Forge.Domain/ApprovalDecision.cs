using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class ApprovalDecision
{
    protected ApprovalDecision() { }

    public int Id { get; private set; }
    public int ApprovalInstanceId { get; private set; }
    public ApprovalInstance ApprovalInstance { get; private set; } = null!;
    public int SequenceOrder { get; private set; }
    public int DecidedByUserId { get; private set; }
    public User DecidedByUser { get; private set; } = null!;
    public DecisionType Decision { get; private set; }
    public string? Comment { get; private set; }
    public DateTime DecidedAt { get; private set; } = DateTime.UtcNow;

    public static ApprovalDecision Create(int approvalInstanceId, int sequenceOrder, int decidedByUserId, DecisionType decision, string? comment)
    {
        if (decision == DecisionType.Rejected && string.IsNullOrWhiteSpace(comment))
            throw new DomainException("A comment is required when rejecting.");

        return new ApprovalDecision
        {
            ApprovalInstanceId = approvalInstanceId,
            SequenceOrder = sequenceOrder,
            DecidedByUserId = decidedByUserId,
            Decision = decision,
            Comment = comment
        };
    }
}