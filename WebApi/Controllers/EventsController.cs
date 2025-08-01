using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Interfaces;

namespace WebApi.Controllers;
[ApiController]
[Route("api/events")]
public class EventsController(IEventService svc) : ControllerBase
{
    private readonly IEventService _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Post(EventDto dto)
    {
        await _svc.PublishAsync(dto);
        return Ok();
    }
}
