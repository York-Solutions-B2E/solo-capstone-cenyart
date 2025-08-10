using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GlobalStatusController(CommunicationDbContext context) : ControllerBase
    {
        private readonly CommunicationDbContext _context = context;

        // GET: api/globalstatus
        [HttpGet]
        public async Task<ActionResult<List<GlobalStatus>>> GetAllGlobalStatuses()
        {
            var globalStatuses = await _context.GlobalStatuses.AsNoTracking().ToListAsync();
            return Ok(globalStatuses);
        }
    }
}
