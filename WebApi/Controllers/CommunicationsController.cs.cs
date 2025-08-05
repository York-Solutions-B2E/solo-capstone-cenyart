using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Enums;
using Shared.Interfaces;

namespace WebApi.Controllers;
[ApiController]
[Route("api/communications")]
public class CommunicationsController(ICommunicationService svc) : ControllerBase
{
    private readonly ICommunicationService _svc = svc;

    [HttpGet]
    public Task<List<CommunicationDto>> GetAll() => _svc.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => (await _svc.GetByIdAsync(id)) is var c && c != null
           ? Ok(c)
           : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CreateCommunicationDto dto)
    {
        var c = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string newStatus)
    {
        await _svc.UpdateStatusAsync(id, newStatus);
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _svc.SoftDeleteAsync(id);
        return NoContent();
    }
}
