using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockMovementsController : ControllerBase
{
    private readonly IStockLedgerService _stockLedgerService;

    public StockMovementsController(IStockLedgerService stockLedgerService)
    {
        _stockLedgerService = stockLedgerService;
    }

    [HttpPost]
    public async Task<ActionResult<StockMovementResult>> Post(PostStockMovementRequest request)
    {
        try
        {
            var result = await _stockLedgerService.PostMovementAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

