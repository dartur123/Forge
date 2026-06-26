using Forge.Domain;
using System.ComponentModel.DataAnnotations;

namespace Forge.Application.Requests
{
    public class PostApprovalRuleRequest
    {
        [MinLength(1)]
        public string EntityType { get; set; } = string.Empty;
        public int RequiredRoleId { get; set; }
        public int SequenceOrder { get; set; }
    }
}
