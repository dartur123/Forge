using Forge.Api.DTOs.Materials;
using Forge.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private static readonly List<Material> Materials = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<List<MaterialResponse>> GetAll()
    {
        var response = Materials.Select(ToResponse).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public ActionResult<MaterialResponse> GetById(int id)
    {
        var material = Materials.FirstOrDefault(m => m.Id == id);
        if (material is null)
            return NotFound();

        return Ok(ToResponse(material));
    }

    [HttpGet("by-type/{type}")]
    public ActionResult<List<MaterialResponse>> GetByType(MaterialType type)
    {
        var filtered = Materials
            .Where(m => m.Type == type)
            .Select(ToResponse)
            .ToList();

        return Ok(filtered);
    }

    [HttpPost]
    public ActionResult<MaterialResponse> Create(CreateMaterialRequest request)
    {
        var material = new Material
        {
            Id = _nextId++,
            Sku = request.Sku,
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            UnitOfMeasure = request.UnitOfMeasure
        };

        Materials.Add(material);
        return CreatedAtAction(nameof(GetById),
            new { id = material.Id }, ToResponse(material));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, CreateMaterialRequest request)
    {
        var existing = Materials.FirstOrDefault(m => m.Id == id);
        if (existing is null)
            return NotFound();

        existing.Sku = request.Sku;
        existing.Name = request.Name;
        existing.Type = request.Type;
        existing.Description = request.Description;
        existing.UnitOfMeasure = request.UnitOfMeasure;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var material = Materials.FirstOrDefault(m => m.Id == id);
        if (material is null)
            return NotFound();

        Materials.Remove(material);
        return NoContent();
    }

    // Private helper — converts a Material entity to a MaterialResponse DTO
    private static MaterialResponse ToResponse(Material material) => new()
    {
        Id = material.Id,
        Sku = material.Sku,
        Name = material.Name,
        Type = material.Type.ToString(),
        Description = material.Description,
        UnitOfMeasure = material.UnitOfMeasure
    };
}