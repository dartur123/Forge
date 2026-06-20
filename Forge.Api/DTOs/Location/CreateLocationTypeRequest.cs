using System.ComponentModel.DataAnnotations;

namespace Forge.Api.DTOs.Location
{
    public class CreateLocationTypeRequest
    {
        [Required]
        [MinLength(1)]
        public string Name { get; set; } = string.Empty;
    }
}
