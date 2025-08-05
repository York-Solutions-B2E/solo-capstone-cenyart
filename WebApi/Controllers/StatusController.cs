using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Interfaces;

namespace WebApi.Controllers;
[ApiController]
[Route("api/statuses")]
public class StatusController(IStatusService svc) : ControllerBase
{
    private readonly IStatusService _svc = svc;

    /// <summary>
    /// GET /api/statuses/{typeCode}
    /// </summary>
    [HttpGet("{typeCode}")]
    public Task<List<StatusOptionDto>> GetForType(string typeCode)
        => _svc.GetForTypeAsync(typeCode);
}
