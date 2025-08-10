using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;

public class GlobalStatusService(CommunicationDbContext context) : IGlobalStatusService
{
    private readonly CommunicationDbContext _context = context;

    public async Task<List<GlobalStatusDto>> GetAllGlobalStatusesAsync()
    {
        return await _context.GlobalStatuses
            .Select(gs => new GlobalStatusDto(gs.StatusCode, gs.Phase, gs.Notes))
            .ToListAsync();
    }
}
