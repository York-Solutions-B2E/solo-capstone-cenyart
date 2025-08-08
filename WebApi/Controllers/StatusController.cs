using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;

namespace Webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController(IStatusService svc) : ControllerBase
{
    [HttpGet("{typeCode}")]
    public Task<IEnumerable<StatusDto>> GetByType(string typeCode) => svc.GetByTypeAsync(typeCode);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StatusCreateDto dto)
    {
        await svc.AddAsync(dto);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] StatusUpdateDto dto)
    {
        await svc.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] StatusDeleteDto dto)
    {
        await svc.DeleteAsync(dto);
        return NoContent();
    }
}
