namespace Forge.Application.Requests
{
    public class ApprovalDecisionRequest
    {
        public int UserId { get; set; }
        public string? Comment { get; set; } = string.Empty;
    }
}
