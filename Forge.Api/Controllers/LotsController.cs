using Forge.Api.DTOs.Lots;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotsController : ControllerBase
{
    private readonly ForgeDbContext _context;

    public LotsController(ForgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LotResponse>>> GetAll()
    {
        var lots = await _context.Lots
            .Include(l => l.Material)
            .Include(l => l.Supplier)
            .Include(l => l.CurrentLocation)
            .ToListAsync();

        return Ok(lots.Select(LotResponse.FromEntity).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LotResponse>> GetById(int id)
    {
        var lot = await _context.Lots
            .Include(l => l.Material)
            .Include(l => l.Supplier)
            .Include(l => l.CurrentLocation)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (lot == null)
        {
            return NotFound();
        }
        return Ok(LotResponse.FromEntity(lot));
    }

    [HttpPost]
    public async Task<ActionResult<LotResponse>> Create(CreateLotRequest createLotRequest)
    {
        var lot = Lot.Create(
            createLotRequest.LotNumber,
            createLotRequest.MaterialId,
            createLotRequest.SupplierId,
            createLotRequest.CurrentLocationId,
            createLotRequest.Quantity,
            createLotRequest.UnitCostPhp,
            DateTime.UtcNow,
            createLotRequest.ExpiryDate
        );
        _context.Lots.Add(lot);
        await _context.SaveChangesAsync();

        var createdLot = await _context.Lots
            .Include(l => l.Material)
            .Include(l => l.Supplier)
            .Include(l => l.CurrentLocation)
            .FirstOrDefaultAsync(l => l.Id == lot.Id);

        return CreatedAtAction(nameof(GetById), new { id = createdLot?.Id }, LotResponse.FromEntity(createdLot!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LotResponse>> Update(int id, CreateLotRequest createLotRequest)
    {
        var lot = await _context.Lots.FindAsync(id);
        if (lot == null)
        {
            return NotFound();
        }
        lot.Update(
            createLotRequest.LotNumber,
            createLotRequest.MaterialId,
            createLotRequest.SupplierId,
            createLotRequest.CurrentLocationId,
            createLotRequest.Quantity,
            createLotRequest.UnitCostPhp,
            DateTime.UtcNow,
            createLotRequest.ExpiryDate
        );
        await _context.SaveChangesAsync();

        var updatedLot = await _context.Lots
            .Include(l => l.Material)
            .Include(l => l.Supplier)
            .Include(l => l.CurrentLocation)
            .FirstOrDefaultAsync(l => l.Id == lot.Id);

        return Ok(LotResponse.FromEntity(updatedLot!));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lot = await _context.Lots.FindAsync(id);
        if (lot == null)
        {
            return NotFound();
        }
        lot.Deactivate();
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

