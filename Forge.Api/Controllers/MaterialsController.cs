using Forge.Api.DTOs.Materials;
using Forge.Domain;
using Forge.Domain.Enums;
using Forge.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly ForgeDbContext _context;

    public MaterialsController(ForgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<MaterialResponse>>> GetAll()
    {
        var materials = await _context.Materials.ToListAsync();
        return Ok(materials.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaterialResponse>> GetById(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material is null)
            return NotFound();

        return Ok(ToResponse(material));
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<List<MaterialResponse>>> GetByType(MaterialType type)
    {
        var filtered = await _context.Materials
            .Where(m => m.Type == type)
            .ToListAsync();

        return Ok(filtered.Select(ToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<MaterialResponse>> Create(CreateMaterialRequest request)
    {
        var material = new Material
        {
            Sku = request.Sku,
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            UnitOfMeasure = request.UnitOfMeasure
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = material.Id }, ToResponse(material));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateMaterialRequest request)
    {
        var existing = await _context.Materials.FindAsync(id);
        if (existing is null)
            return NotFound();

        existing.Sku = request.Sku;
        existing.Name = request.Name;
        existing.Type = request.Type;
        existing.Description = request.Description;
        existing.UnitOfMeasure = request.UnitOfMeasure;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material is null)
            return NotFound();

        material.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static MaterialResponse ToResponse(Material material) => new()
    {
        Id = material.Id,
        Sku = material.Sku,
        Name = material.Name,
        Type = material.Type.ToString(),
        Description = material.Description,
        UnitOfMeasure = material.UnitOfMeasure,
        IsActive = material.IsActive
    };
}