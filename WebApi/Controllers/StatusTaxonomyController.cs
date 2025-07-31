using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;
using Shared.Enums;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StatusTaxonomyController : ControllerBase
{
    private readonly IStatusTaxonomyService _statusTaxonomyService;

    public StatusTaxonomyController(IStatusTaxonomyService statusTaxonomyService)
    {
        _statusTaxonomyService = statusTaxonomyService;
    }

    [HttpGet("global-statuses")]
    public async Task<ActionResult<List<GlobalStatusDto>>> GetAllGlobalStatuses()
    {
        try
        {
            var result = await _statusTaxonomyService.GetAllGlobalStatusesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("phases")]
    public ActionResult<List<StatusPhaseDto>> GetPhases()
    {
        try
        {
            // Get all enum values and convert to DTOs
            var phases = Enum.GetValues<StatusPhase>()
                .Select(phase => new StatusPhaseDto
                {
                    Phase = phase,
                    DisplayName = phase.ToString()
                })
                .ToList();

            return Ok(phases);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
