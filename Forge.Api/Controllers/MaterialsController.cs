using Forge.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    // Temporary in-memory "database" — just a list living in memory.
    // It resets every time you stop and restart the app. Real database comes in Week 2.
    private static readonly List<Material> Materials = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<List<Material>> GetAll()
    {
        return Ok(Materials);
    }

    [HttpGet("by-type/{type}")]
    public ActionResult<List<Material>> GetByType(MaterialType type)
    {
        var filtered = Materials.Where(m => m.Type == type).ToList();
        return Ok(filtered);
    }

    [HttpGet("{id}")]
    public ActionResult<Material> GetById(int id)
    {
        var material = Materials.FirstOrDefault(m => m.Id == id);
        if (material is null)
            return NotFound();

        return Ok(material);
    }

    [HttpPost]
    public ActionResult<Material> Create(Material material)
    {
        material.Id = _nextId++;
        Materials.Add(material);
        return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Material updated)
    {
        var existing = Materials.FirstOrDefault(m => m.Id == id);
        if (existing is null)
            return NotFound();

        existing.Sku = updated.Sku;
        existing.Name = updated.Name;
        existing.Type = updated.Type;
        existing.UnitOfMeasure = updated.UnitOfMeasure;

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
}