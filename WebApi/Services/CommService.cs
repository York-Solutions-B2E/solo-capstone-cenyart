using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;

public class CommService(CommunicationDbContext context) : ICommService
{
    private readonly CommunicationDbContext _context = context;

    public async Task<PaginatedResult<CommDto>> GetCommunicationsAsync(int pageNumber, int pageSize)
    {
        var query = _context.Communications
            .AsNoTracking()
            .OrderByDescending(c => c.LastUpdatedUtc);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(c => new CommDto(
            c.Id,
            c.Title,
            c.TypeCode,
            c.CurrentStatusCode,
            c.LastUpdatedUtc
        )).ToList();

        return new PaginatedResult<CommDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<CommDetailsDto?> GetCommunicationByIdAsync(Guid id)
    {
        var comm = await _context.Communications
            .AsNoTracking()
            .Include(c => c.StatusHistory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comm == null) return null;

        var statusHistoryDtos = comm.StatusHistory
            .OrderBy(h => h.OccurredUtc)
            .Select(h => new StatusHistoryDto(h.StatusCode, h.OccurredUtc))
            .ToList();

        return new CommDetailsDto(
            comm.Id,
            comm.Title,
            comm.TypeCode,
            comm.CurrentStatusCode,
            comm.LastUpdatedUtc,
            statusHistoryDtos
        );
    }

    public async Task<Guid> CreateCommunicationAsync(CreateCommPayload payload)
    {
        // Validate Type exists and is active
        var typeExists = await _context.Types
            .AnyAsync(t => t.TypeCode == payload.TypeCode && t.IsActive);

        if (!typeExists)
            throw new ArgumentException($"Communication Type '{payload.TypeCode}' does not exist or is inactive.");

        // Validate Status is allowed and active for this type
        var statusValid = await _context.Statuses
            .AnyAsync(s => s.TypeCode == payload.TypeCode
                        && s.StatusCode == payload.CurrentStatusCode
                        && s.IsActive);

        if (!statusValid)
            throw new ArgumentException($"Status '{payload.CurrentStatusCode}' is not valid for Type '{payload.TypeCode}'.");

        var entity = new Communication
        {
            Id = Guid.NewGuid(),
            Title = payload.Title,
            TypeCode = payload.TypeCode,
            CurrentStatusCode = payload.CurrentStatusCode,
            LastUpdatedUtc = DateTime.UtcNow
        };

        _context.Communications.Add(entity);

        var history = new StatusHistory
        {
            Id = Guid.NewGuid(),
            CommunicationId = entity.Id,
            StatusCode = entity.CurrentStatusCode,
            OccurredUtc = entity.LastUpdatedUtc
        };

        _context.StatusHistories.Add(history);

        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> AddStatusToHistoryAsync(CommEventPayload evt)
    {
        var comm = await _context.Communications
            .Include(c => c.StatusHistory)
            .FirstOrDefaultAsync(c => c.Id == evt.CommunicationId);

        if (comm == null) return false;

        comm.StatusHistory.Add(new StatusHistory
        {
            CommunicationId = evt.CommunicationId,
            StatusCode = evt.StatusCode,
            OccurredUtc = evt.OccurredUtc
        });

        comm.CurrentStatusCode = evt.StatusCode;
        comm.LastUpdatedUtc = evt.OccurredUtc;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
