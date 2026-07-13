using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Microsoft.AspNetCore.Mvc;


namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierResult>>> GetAll()
    {
        return await _supplierService.GetAllSuppliersAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierResult>> GetById(int id)
    {
        try
        {
            return await _supplierService.GetSupplierAsync(id);
        }
        catch(NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierResult>> Create(PostSupplierRequest request)
    {
        var createdSupplier = await _supplierService.CreateSupplierAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = createdSupplier.Id }, createdSupplier);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PostSupplierRequest request)
    {
        try
        {
            var updatedSupplier = await _supplierService.UpdateSupplierAsync(id, request);

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
            await _supplierService.DeactivateSupplierAsync(id);
            return NoContent();
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }
}

