using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotsController : ControllerBase
{
    private readonly ILotService _lotService;

    public LotsController(ILotService lotService)
    {
        _lotService = lotService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LotResult>>> GetAll()
    {
        return await _lotService.GetAllLotsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LotResult>> GetById(int id)
    {
        try
        {
            return await _lotService.GetLotByIdAsync(id);
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<LotResult>> Create(PostLotRequest request)
    {
        var createdLot = await _lotService.CreateLotAsync(request);

        return CreatedAtAction(nameof(GetById),
            new { id = createdLot.Id }, createdLot);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PostLotRequest request)
    {
        try
        {
            var updatedLot = await _lotService.UpdateLotAsync(id, request);

            return NoContent();
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
        catch (InvalidOperationException ioe)
        {
            return BadRequest(ioe.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _lotService.DeactivateLotAsync(id);

            return NoContent();
        }
        catch (NotFoundException nfe)
        {
            return NotFound(nfe.Message);
        }
        catch (InvalidOperationException ioe)
        {
            return BadRequest(ioe.Message);
        }
    }
}