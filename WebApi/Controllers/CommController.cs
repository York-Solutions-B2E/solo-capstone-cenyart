using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;
using Shared.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    public class CommController(ICommService commService) : ControllerBase
    {
        private readonly ICommService _commService = commService;

        // [Authorize(Policy = "User")]
        // [Authorize(Policy = "Admin")]
        [HttpGet] // GET: api/comms?pageNumber=1&pageSize=10
        public async Task<IActionResult> GetCommunications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Page number and size must be greater than zero.");

            var result = await _commService.GetCommunicationsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        // [Authorize(Policy = "User")]
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")] // GET: api/comms/{id}
        public async Task<IActionResult> GetCommunicationById(Guid id)
        {
            var comm = await _commService.GetCommunicationByIdAsync(id);
            if (comm == null)
                return NotFound();

            return Ok(comm);
        }

        [Authorize(Policy = "User")]
        [Authorize(Policy = "Admin")]
        [HttpPost] // POST: api/comms
        public async Task<IActionResult> CreateCommunication([FromBody] CreateCommPayload payload)
        {
            if (payload == null)
                return BadRequest("Payload cannot be null.");

            try
            {
                var newId = await _commService.CreateCommunicationAsync(payload);
                return CreatedAtAction(nameof(GetCommunicationById), new { id = newId }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
