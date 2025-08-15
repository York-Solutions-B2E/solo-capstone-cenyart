using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]es")]
    public class GlobalStatusController(CommunicationDbContext context) : ControllerBase
    {
        private readonly CommunicationDbContext _context = context;

        [Authorize(Policy = "User")]
        [Authorize(Policy = "Admin")]
        [HttpGet] // GET: api/globalstatuses
        public async Task<ActionResult<List<GlobalStatus>>> GetAllGlobalStatuses()
        {
            var globalStatuses = await _context.GlobalStatuses.AsNoTracking().ToListAsync();
            return Ok(globalStatuses);
        }
    }
}
