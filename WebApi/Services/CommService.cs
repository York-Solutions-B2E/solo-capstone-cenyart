using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;

public class CommService(CommunicationDbContext db, IValidationService validator) : ICommService
{
    public async Task CreateAsync(CommunicationCreateDto dto)
    {
        // Ensure "Pending" exists for this type
        await validator.ValidateStatusForTypeAsync(dto.TypeCode, "Pending");

        var now = DateTime.UtcNow;
        var comm = new Communication
        {
            Id             = Guid.NewGuid(),
            Title          = dto.Title,
            TypeCode       = dto.TypeCode,
            CurrentStatus  = "Pending",
            LastUpdatedUtc = now,
            StatusHistory  = new List<CommunicationStatusHistory>
            {
                new() { StatusCode = "Pending", OccurredUtc = now }
            }
        };

        db.Communications.Add(comm);
        await db.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Dto> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
    {
        var q     = db.Communications.Where(c => c.IsActive).OrderByDescending(c => c.LastUpdatedUtc);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .Select(c => new Dto(c.Id, c.Title, c.TypeCode, c.CurrentStatus, c.LastUpdatedUtc))
                           .ToListAsync();
        return (items, total);
    }

    public async Task<IEnumerable<Dto>> GetAllAsync()
        => await db.Communications
                 .Where(c => c.IsActive)
                 .OrderByDescending(c => c.LastUpdatedUtc)
                 .Select(c => new Dto(c.Id, c.Title, c.TypeCode, c.CurrentStatus, c.LastUpdatedUtc))
                 .ToListAsync();

    public async Task<DetailsDto> GetByIdAsync(Guid id)
    {
        var c = await db.Communications
                        .Include(c => c.StatusHistory)
                        .FirstOrDefaultAsync(c => c.Id == id && c.IsActive)
              ?? throw new KeyNotFoundException($"Communication {id} not found.");

        var history = c.StatusHistory
                       .OrderBy(h => h.OccurredUtc)
                       .Select(h => new StatusHistoryDto(h.StatusCode, h.OccurredUtc))
                       .ToList();

        return new DetailsDto(c.Id, c.Title, c.TypeCode, c.CurrentStatus, c.LastUpdatedUtc, history);
    }

    public async Task UpdateAsync(CommunicationUpdateDto dto)
    {
        var comm = await db.Communications
                           .Include(c => c.StatusHistory)
                           .FirstOrDefaultAsync(c => c.Id == dto.Id && c.IsActive)
                   ?? throw new KeyNotFoundException($"Communication {dto.Id} not found.");

        // Validate new status for this communication’s type
        await validator.ValidateStatusForTypeAsync(comm.TypeCode, dto.NewStatus);

        var now = DateTime.UtcNow;
        comm.CurrentStatus   = dto.NewStatus;
        comm.LastUpdatedUtc  = now;
        comm.StatusHistory.Add(new CommunicationStatusHistory
        {
            StatusCode  = dto.NewStatus,
            OccurredUtc = now
        });

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(CommunicationDeleteDto dto)
    {
        var comm = await db.Communications.FindAsync(dto.Id)
                   ?? throw new KeyNotFoundException($"Communication {dto.Id} not found.");
        comm.IsActive = false;
        await db.SaveChangesAsync();
    }
}
