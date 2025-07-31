using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Exceptions;
using Shared.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CommunicationTypesController : ControllerBase
{
    private readonly ICommunicationTypeService _communicationTypeService;
    private readonly IStatusTaxonomyService _statusTaxonomyService;

    public CommunicationTypesController(
        ICommunicationTypeService communicationTypeService,
        IStatusTaxonomyService statusTaxonomyService)
    {
        _communicationTypeService = communicationTypeService;
        _statusTaxonomyService = statusTaxonomyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CommunicationTypeDto>>> GetAll(bool includeInactive = false)
    {
        try
        {
            var result = await _communicationTypeService.GetAllAsync(includeInactive);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{typeCode}")]
    public async Task<ActionResult<CommunicationTypeDto>> GetByCode(string typeCode)
    {
        try
        {
            var result = await _communicationTypeService.GetByCodeAsync(typeCode);
            if (result == null)
                return NotFound($"Communication type '{typeCode}' not found");

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CommunicationTypeDto>> Create(CreateCommunicationDto request)
    {
        try
        {
            var result = await _communicationTypeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetByCode), new { typeCode = result.TypeCode }, result);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{typeCode}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CommunicationTypeDto>> Update(string typeCode, UpdateCommunicationDto request)
    {
        try
        {
            var result = await _communicationTypeService.UpdateAsync(typeCode, request);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{typeCode}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SoftDelete(string typeCode)
    {
        try
        {
            await _communicationTypeService.SoftDeleteAsync(typeCode);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("{typeCode}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Restore(string typeCode)
    {
        try
        {
            await _communicationTypeService.RestoreAsync(typeCode);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{typeCode}/statuses")]
    public async Task<ActionResult<List<GlobalStatusDto>>> GetValidStatuses(string typeCode)
    {
        try
        {
            var communicationType = await _communicationTypeService.GetByCodeAsync(typeCode);
            if (communicationType == null)
                return NotFound($"Communication type '{typeCode}' not found");

            return Ok(communicationType.ValidStatuses);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{typeCode}/statuses")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateStatusMappings(string typeCode, [FromBody] List<string> statusCodes)
    {
        try
        {
            // First verify the communication type exists
            var communicationType = await _communicationTypeService.GetByCodeAsync(typeCode);
            if (communicationType == null)
                return NotFound($"Communication type '{typeCode}' not found");

            // Update the status mappings through the status taxonomy service
            await _statusTaxonomyService.UpdateTypeStatusMappingsAsync(typeCode, statusCodes);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
