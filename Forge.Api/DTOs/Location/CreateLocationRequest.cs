using System.ComponentModel.DataAnnotations;

namespace Forge.Api.DTOs.Location
{
    public class CreateLocationRequest
    {

        [Required]
        public string Name { get; set; } = string.Empty;
        public int LocationTypeId { get; set; }
        public int? ParentLocationId { get; set; }

    }
}
