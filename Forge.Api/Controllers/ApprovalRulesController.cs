using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalRulesController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalRulesController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet("{entityType}")]
    public async Task<ActionResult<List<ApprovalRequirement>>> GetApprovalRules(string entityType)
    {
        var rules = await _approvalService.GetRequiredApprovalsAsync(entityType);
        return Ok(rules);
    }

    [HttpPost]
    public async Task<ActionResult<ApprovalRuleResult>> CreateApprovalRule(PostApprovalRuleRequest request)
    {
        var rule = await _approvalService.CreateRuleAsync(request);
        return CreatedAtAction(nameof(GetApprovalRules), new { entityType = rule.EntityType }, rule);
    }

    [HttpGet("rule/{id}")]
    public async Task<ActionResult<ApprovalRuleResult>> GetApprovalRule(int id)
    {
        try
        {
            var rule = await _approvalService.GetRuleAsync(id);
            return Ok(rule);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("rule/{id}")]
    public async Task<IActionResult> DeactivateApprovalRule(int id)
    {
        try
        {
            await _approvalService.DeactivateRuleAsync(id);
        }
        catch (NotFoundException ex)
        { 
            return NotFound(ex.Message);
        }
        
        return NoContent();
    }
}