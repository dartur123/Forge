using System.ComponentModel.DataAnnotations;

namespace Forge.Api.DTOs.Location
{
    public class LocationTypeResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
