using Forge.Domain.Enums;

namespace Forge.Domain;

public class ApprovalRule
{
    public int Id { get; set; }
    public ApprovableEntityType EntityType { get; set; }
    public decimal ThresholdAmountPhp { get; set; }
    public int RequiredRoleId { get; set; }
    public int SequenceOrder { get; set; } = 1;
}