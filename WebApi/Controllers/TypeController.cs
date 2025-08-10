using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]s")] // -> "api/types"
public class TypeController(ITypeService typeService) : ControllerBase
{
    private readonly ITypeService _typeService = typeService;

    // GET api/types
    [HttpGet]
    public async Task<ActionResult<List<TypeDto>>> GetAll()
    {
        var list = await _typeService.GetAllTypesAsync();
        return Ok(list);
    }

    // GET api/types/{typeCode}
    [HttpGet("{typeCode}")]
    public async Task<ActionResult<TypeDetailsDto?>> GetByCode(string typeCode)
    {
        var dto = await _typeService.GetTypeByCodeAsync(typeCode);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    // POST api/types
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTypePayload payload)
    {
        await _typeService.CreateTypeAsync(payload);
        return CreatedAtAction(nameof(GetByCode), new { typeCode = payload.TypeCode }, null);
    }

    // PUT api/types/{typeCode}
    [HttpPut("{typeCode}")]
    public async Task<IActionResult> UpdateTypeAsync(string typeCode, [FromBody] UpdateTypePayload payload)
    {
        if (payload == null || typeCode != payload.TypeCode)
            return BadRequest();

        // if (!await _typeService.ValidateStatusesForTypeAsync(payload.TypeCode, payload.AllowedStatusCodes ?? []))
        //     return BadRequest("Invalid status codes for this type.");

        await _typeService.UpdateTypeAsync(payload);
        return NoContent();
    }


    // DELETE api/types/{typeCode}
    [HttpDelete("{typeCode}")]
    public async Task<IActionResult> SoftDelete(string typeCode)
    {
        await _typeService.SoftDeleteTypeAsync(new DeleteTypePayload(typeCode));
        return NoContent();
    }
}
