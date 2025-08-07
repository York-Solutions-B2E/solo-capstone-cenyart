using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;

namespace Webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommController(ICommService comm) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPage([FromQuery]int page = 1, [FromQuery]int pageSize = 10)
    {
        var (items, total) = await comm.GetPaginatedAsync(page, pageSize);
        return Ok(new { Items = items, TotalCount = total });
    }

    [HttpGet("all")]
    public Task<IEnumerable<Dto>> GetAll() => comm.GetAllAsync();

    [HttpGet("{id:guid}")]
    public Task<DetailsDto> GetById(Guid id) => comm.GetByIdAsync(id);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommunicationCreateDto dto)
    {
        await comm.CreateAsync(dto);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CommunicationUpdateDto dto)
    {
        await comm.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] CommunicationDeleteDto dto)
    {
        await comm.DeleteAsync(dto);
        return NoContent();
    }
}
