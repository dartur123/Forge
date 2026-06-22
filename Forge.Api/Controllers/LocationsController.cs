using Forge.Api.DTOs.Location;
using Forge.Domain;
using Forge.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ForgeDbContext _context;

    public LocationsController(ForgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationResponse>>> GetAll()
    {
        var locations = await _context.Locations
            .Include(l => l.LocationType)
            .Include(l => l.ParentLocation)
            .ToListAsync();

        return Ok(locations.Select(LocationResponse.FromEntity).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationResponse>> GetById(int id)
    {
        var location = await _context.Locations
            .Include(l => l.LocationType)
            .Include(l => l.ParentLocation)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null)
            return NotFound();

        return Ok(LocationResponse.FromEntity(location));
    }

    [HttpPost]
    public async Task<ActionResult<LocationResponse>> Create(CreateLocationRequest request)
    {
        var location = new Location
        {
            Name = request.Name,
            LocationTypeId = request.LocationTypeId,
            ParentLocationId = request.ParentLocationId
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var created = await _context.Locations
            .Include(l => l.LocationType)
            .Include(l => l.ParentLocation)
            .FirstOrDefaultAsync(l => l.Id == location.Id);

        return CreatedAtAction(nameof(GetById),
            new { id = location.Id }, LocationResponse.FromEntity(created!));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateLocationRequest request)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null)
            return NotFound();

        location.Name = request.Name;
        location.LocationTypeId = request.LocationTypeId;
        location.ParentLocationId = request.ParentLocationId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null)
            return NotFound();

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}