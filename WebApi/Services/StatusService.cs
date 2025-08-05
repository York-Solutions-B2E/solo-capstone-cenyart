using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;
public class StatusService(CommunicationDbContext db) : IStatusService
{
    private readonly CommunicationDbContext _db = db;

    public async Task<List<StatusOptionDto>> GetForTypeAsync(string typeCode)
    {
        return await _db.CommunicationTypeStatuses
            .Where(cts => cts.TypeCode == typeCode && cts.IsActive)
            .Include(cts => cts.Status)
            .OrderBy(cts => cts.SortOrder)
            .Select(cts => new StatusOptionDto {
                StatusCode  = cts.StatusCode,
                DisplayName = cts.Status.DisplayName,
                SortOrder   = cts.SortOrder
            })
            .ToListAsync();
    }
}
