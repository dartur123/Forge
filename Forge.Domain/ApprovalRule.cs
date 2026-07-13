using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class ApprovalRule
{
    protected ApprovalRule() { }

    public int Id { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public int RequiredRoleId { get; private set; }
    public Role RequiredRole { get; private set; } = null!;
    public int SequenceOrder { get; private set; }
    public bool IsActive { get; private set; }

    public static ApprovalRule Create(string entityType, int requiredRoleId, int sequenceOrder)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new DomainException("Entity type is required.");

        if (sequenceOrder < 1)
            throw new DomainException("Sequence order must be at least 1.");

        return new ApprovalRule
        {
            EntityType = entityType,
            RequiredRoleId = requiredRoleId,
            SequenceOrder = sequenceOrder,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
}