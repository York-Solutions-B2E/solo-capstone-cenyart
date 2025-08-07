using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;

public class StatusService(CommunicationDbContext db) : IStatusService
{
    public async Task<IEnumerable<StatusDto>> GetByTypeAsync(string typeCode)
        => await db.CommunicationTypeStatuses
                 .Where(s => s.TypeCode == typeCode)
                 .Select(s => new StatusDto(s.Id, s.TypeCode, s.StatusCode, s.Description, s.IsActive))
                 .ToListAsync();

    public async Task AddAsync(StatusCreateDto dto)
    {
        db.CommunicationTypeStatuses.Add(new CommunicationTypeStatus
        {
            TypeCode    = dto.TypeCode,
            StatusCode  = dto.StatusCode,
            Description = dto.Description,
            IsActive    = true
        });
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(StatusUpdateDto dto)
    {
        var s = await db.CommunicationTypeStatuses.FindAsync(dto.Id)
              ?? throw new KeyNotFoundException($"Status {dto.Id} not found.");

        s.StatusCode  = dto.StatusCode;
        s.Description = dto.Description;
        s.IsActive    = dto.IsActive;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(StatusDeleteDto dto)
    {
        var s = await db.CommunicationTypeStatuses.FindAsync(dto.Id)
              ?? throw new KeyNotFoundException($"Status {dto.Id} not found.");

        db.CommunicationTypeStatuses.Remove(s);
        await db.SaveChangesAsync();
    }
}
