using Forge.Application.Exceptions;
using Forge.Application.Interfaces;
using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain.Exceptions;
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

    [HttpPost("instances")]
    public async Task<ActionResult<ApprovalInstanceResult>> StartApproval(StartApprovalRequest request)
    {
        try
        {
            var instance = await _approvalService.StartApprovalAsync(request.EntityType, request.EntityId);
            return CreatedAtAction(nameof(GetInstance), new { id = instance.Id }, instance);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("instances/{id}")]
    public async Task<ActionResult<ApprovalInstanceResult>> GetInstance(int id)
    {
        try
        {
            var instance = await _approvalService.GetInstanceAsync(id);
            return Ok(instance);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("instances/{id}/approve")]
    public async Task<ActionResult<ApprovalInstanceResult>> ApproveStep(int id, ApprovalDecisionRequest request)
    {
        try
        {
            var instance = await _approvalService.ApproveStepAsync(id, request.UserId, request.Comment);
            return Ok(instance);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("instances/{id}/reject")]
    public async Task<ActionResult<ApprovalInstanceResult>> RejectStep(int id, ApprovalDecisionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
            return BadRequest("A comment is required when rejecting.");

        try
        {
            var instance = await _approvalService.RejectStepAsync(id, request.UserId, request.Comment);
            return Ok(instance);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("instances/{id}/resubmit")]
    public async Task<ActionResult<ApprovalInstanceResult>> Resubmit(int id)
    {
        try
        {
            var instance = await _approvalService.ResubmitAsync(id);
            return Ok(instance);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}