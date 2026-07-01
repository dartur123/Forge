using System;
using System.Collections.Generic;
using System.Text;

namespace Forge.Application.Responses
{
    public class LocationResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public static LocationResult FromEntity(Forge.Domain.Location location) => new()
        {
            Id = location.Id,
            Name = location.Name
        };
    }
}
