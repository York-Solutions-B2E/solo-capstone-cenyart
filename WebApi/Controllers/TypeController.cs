using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;

namespace Webapi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TypeController(ITypeService types) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<TypeDto>> GetAll() => types.GetAllAsync();

    [HttpGet("{code}")]
    public Task<TypeDetailsDto> Get(string code) => types.GetByCodeAsync(code);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TypeCreateDto dto)
    {
        await types.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { code = dto.TypeCode }, null);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] TypeUpdateDto dto)
    {
        await types.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] TypeDeleteDto dto)
    {
        await types.DeleteAsync(dto);
        return NoContent();
    }
}
