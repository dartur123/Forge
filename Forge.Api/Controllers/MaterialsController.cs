using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly IMaterialService _materialService;

    public MaterialsController(IMaterialService materialService)
    {
        _materialService = materialService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MaterialResult>>> GetAll()
    {
        return await _materialService.GetAllMaterialsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaterialResult>> GetById(int id)
    {
        try
        {
            return await _materialService.GetMaterialAsync(id);
        }
        catch(NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<List<MaterialResult>>> GetByType(MaterialType type)
    {
        return await _materialService.GetMaterialByTypeAsync(type);
    }

    [HttpPost]
    public async Task<ActionResult<MaterialResult>> Create(PostMaterialRequest request)
    {
        var createdMaterial = await _materialService.CreateMaterialAsync(request);

        return CreatedAtAction(nameof(GetById),
            new { id = createdMaterial.Id }, createdMaterial);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PostMaterialRequest request)
    {
        try
        {
            var updatedMaterial = await _materialService.UpdateMaterialAsync(id, request);

            return NoContent();
        }
        catch(NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _materialService.DeactivateMaterialAsync(id);

            return NoContent();
        }
        catch(NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }
}