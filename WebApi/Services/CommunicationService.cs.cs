using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;
public class CommunicationService(CommunicationDbContext db) : ICommunicationService
{
    private readonly CommunicationDbContext _db = db;

    public async Task<List<CommunicationDto>> GetAllAsync() =>
        await _db.Communications
                 .Select(c => new CommunicationDto {
                     Id = c.Id,
                     Title = c.Title,
                     TypeCode = c.TypeCode,
                     CurrentStatus = c.CurrentStatus,
                     LastUpdatedUtc = c.LastUpdatedUtc
                 })
                 .ToListAsync();

    public async Task<CommunicationDto> GetByIdAsync(Guid id)
    {
        var c = await _db.Communications
                        .Include(c => c.StatusHistory)
                        .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            throw new KeyNotFoundException($"Communication with ID {id} not found.");

        return new CommunicationDto {
            Id = c.Id,
            Title = c.Title,
            TypeCode = c.TypeCode,
            CurrentStatus = c.CurrentStatus,
            LastUpdatedUtc = c.LastUpdatedUtc,
            History = c.StatusHistory
                    .Where(h => h.IsActive)
                    .OrderBy(h => h.OccurredUtc)
                    .Select(h => new StatusHistoryDto {
                        Id = h.Id,
                        StatusCode = h.StatusCode,
                        OccurredUtc = h.OccurredUtc
                    })
                    .ToList()
        };
    }


    public async Task<CommunicationDto> CreateAsync(CreateCommunicationDto dto)
    {
        var comm = new Communication {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            TypeCode = dto.TypeCode,
            CurrentStatus = "ReadyForRelease",
            LastUpdatedUtc = DateTime.UtcNow
        };
        _db.Communications.Add(comm);
        _db.CommunicationStatusHistory.Add(new CommunicationStatusHistory {
            Id = Guid.NewGuid(),
            CommunicationId = comm.Id,
            StatusCode = comm.CurrentStatus,
            OccurredUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return await GetByIdAsync(comm.Id)!;
    }

    public async Task UpdateStatusAsync(Guid id, string newStatus)
    {
        // 1️⃣ (Optional) Validate that this enum value is actually seeded in GlobalStatuses
        var exists = await _db.GlobalStatuses
                            .AnyAsync(gs => gs.StatusCode == newStatus);
        if (!exists)
            throw new KeyNotFoundException($"Status code '{newStatus}' is not defined in GlobalStatuses.");

        // 2️⃣ Fetch and update the communication
        var comm = await _db.Communications.FindAsync(id);
        if (comm == null || !comm.IsActive)
            throw new InvalidOperationException($"Communication '{id}' not found.");

        comm.CurrentStatus  = newStatus;
        comm.LastUpdatedUtc = DateTime.UtcNow;

        // 3️⃣ Add history record
        _db.CommunicationStatusHistory.Add(new CommunicationStatusHistory
        {
            Id              = Guid.NewGuid(),
            CommunicationId = id,
            StatusCode      = newStatus,
            OccurredUtc     = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }



    public async Task SoftDeleteAsync(Guid id)
    {
        var comm = await _db.Communications.FindAsync(id);
        if (comm == null) return;
        comm.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
