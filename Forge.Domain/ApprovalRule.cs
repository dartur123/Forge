namespace Forge.Domain;

public class ApprovalRule
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int RequiredRoleId { get; set; }
    public Role RequiredRole { get; set; } = null!;
    public int SequenceOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}