using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class ApprovalInstance
{
    protected ApprovalInstance() { }

    public int Id { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public int EntityId { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public int CurrentSequenceOrder { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public List<ApprovalDecision> Decisions { get; private set; } = new();

    public static ApprovalInstance Create(string entityType, int entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("Entity type is required.");

        return new ApprovalInstance
        {
            EntityType = entityType,
            EntityId = entityId,
            Status = ApprovalStatus.Pending,
            CurrentSequenceOrder = 1
        };
    }

    public void AdvanceToNextStep()
    {
        CurrentSequenceOrder++;
    }

    public void Approve()
    {
        Status = ApprovalStatus.Approved;
    }

    public void Reject()
    {
        Status = ApprovalStatus.Rejected;
    }

    public void Resubmit()
    {
        if (Status != ApprovalStatus.Rejected)
            throw new DomainException("Only rejected instances can be resubmitted.");

        Status = ApprovalStatus.Pending;
        CurrentSequenceOrder = 1;
    }
}