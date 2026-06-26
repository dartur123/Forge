using Forge.Domain;

namespace Forge.Application.Responses
{
    public class RoleResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public static RoleResult FromEntity(Role role) => new()
        {
            Id = role.Id,
            Name = role.Name
        };
    }
}
