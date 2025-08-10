using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TypeController(ITypeService typeService) : ControllerBase
    {
        private readonly ITypeService _typeService = typeService;

        // GET: api/type
        [HttpGet]
        public async Task<ActionResult<List<TypeDto>>> GetAllTypes()
        {
            var types = await _typeService.GetAllTypesAsync();
            return Ok(types);
        }

        // GET: api/type/{typeCode}
        [HttpGet("{typeCode}")]
        public async Task<ActionResult<TypeDetailsDto>> GetTypeByCode(string typeCode)
        {
            var type = await _typeService.GetTypeByCodeAsync(typeCode);
            if (type is null)
                return NotFound();

            return Ok(type);
        }

        // POST: api/type
        [HttpPost]
        public async Task<IActionResult> CreateType([FromBody] CreateTypePayload payload)
        {
            if (payload == null)
                return BadRequest("Payload cannot be null.");

            try
            {
                await _typeService.CreateTypeAsync(payload);
                return CreatedAtAction(nameof(GetTypeByCode), new { typeCode = payload.TypeCode }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/type
        [HttpPut]
        public async Task<IActionResult> UpdateType([FromBody] UpdateTypePayload payload)
        {
            if (payload == null)
                return BadRequest("Payload cannot be null.");

            try
            {
                await _typeService.UpdateTypeAsync(payload);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/type
        [HttpDelete]
        public async Task<IActionResult> SoftDeleteType([FromBody] DeleteTypePayload payload)
        {
            if (payload == null)
                return BadRequest("Payload cannot be null.");

            try
            {
                await _typeService.SoftDeleteTypeAsync(payload);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/type/validate-statuses
        [HttpPost("validate-statuses")]
        public async Task<ActionResult<bool>> ValidateStatusesForType([FromBody] ValidateStatusesPayload payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.TypeCode) || payload.StatusCodes == null)
                return BadRequest("Invalid payload.");

            bool isValid = await _typeService.ValidateStatusesForTypeAsync(payload.TypeCode, payload.StatusCodes);
            return Ok(isValid);
        }
    }
}
