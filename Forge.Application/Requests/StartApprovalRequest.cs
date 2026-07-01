using System.ComponentModel.DataAnnotations;

namespace Forge.Application.Requests
{
    public class StartApprovalRequest
    {
        public int EntityId { get; set; }

        [MinLength(1)]
        public string EntityType { get; set; } = string.Empty;
    }
}
