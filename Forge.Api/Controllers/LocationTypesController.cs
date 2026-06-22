using Forge.Api.DTOs.Location;
using Forge.Api.DTOs.Materials;
using Forge.Domain;
using Forge.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationTypesController : ControllerBase
    {
        private readonly ForgeDbContext _context;

        public LocationTypesController(ForgeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<LocationTypeResponse>>> GetAll()
        {
            var locationTypes = await _context.LocationTypes.ToListAsync();
            return Ok(locationTypes.Select(ToResponse).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationTypeResponse>> GetById(int id)
        {
            var locationType = await _context.LocationTypes.FindAsync(id);
            if (locationType is null)
                return NotFound();
            return Ok(ToResponse(locationType));
        }

        [HttpPost]
        public async Task<ActionResult<LocationTypeResponse>> Create(CreateLocationTypeRequest request)
        {
            var locationType = new LocationType
            {
                Name = request.Name
            };
            _context.LocationTypes.Add(locationType);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = locationType.Id }, ToResponse(locationType));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<LocationTypeResponse>> Update(int id, CreateLocationTypeRequest request)
        {
            var locationType = await _context.LocationTypes.FindAsync(id);
            if (locationType is null)
                return NotFound();
            locationType.Name = request.Name;
            await _context.SaveChangesAsync();
            return Ok(ToResponse(locationType));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var locationType = await _context.LocationTypes.FindAsync(id);
            if (locationType is null)
                return NotFound();

            locationType.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public static LocationTypeResponse ToResponse(LocationType locationType) => new()
        {
            Id = locationType.Id,
            Name = locationType.Name
        };
    }
}
