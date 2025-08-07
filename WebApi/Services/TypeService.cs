using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;

public class TypeService(CommunicationDbContext db) : ITypeService
{
    public async Task CreateAsync(TypeCreateDto dto)
    {
        if (await db.CommunicationTypes.AnyAsync(t => t.TypeCode == dto.TypeCode))
            throw new InvalidOperationException($"Type '{dto.TypeCode}' already exists.");

        db.CommunicationTypes.Add(new CommunicationType
        {
            TypeCode    = dto.TypeCode,
            DisplayName = dto.DisplayName,
            IsActive    = true
        });
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<TypeDto>> GetAllAsync()
        => await db.CommunicationTypes
                 .Select(t => new TypeDto(t.TypeCode, t.DisplayName, t.IsActive))
                 .ToListAsync();

    public async Task<TypeDetailsDto> GetByCodeAsync(string typeCode)
    {
        var t = await db.CommunicationTypes
                       .Include(t => t.ValidStatuses)
                       .FirstOrDefaultAsync(t => t.TypeCode == typeCode)
              ?? throw new KeyNotFoundException($"Type '{typeCode}' not found.");

        var statuses = t.ValidStatuses
                        .Select(s => new StatusDto(s.Id, s.TypeCode, s.StatusCode, s.Description, s.IsActive))
                        .ToList();

        return new TypeDetailsDto(t.TypeCode, t.DisplayName, t.IsActive, statuses);
    }

    public async Task UpdateAsync(TypeUpdateDto dto)
    {
        var t = await db.CommunicationTypes.FindAsync(dto.TypeCode)
             ?? throw new KeyNotFoundException($"Type '{dto.TypeCode}' not found.");

        t.DisplayName = dto.DisplayName;
        t.IsActive    = dto.IsActive;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TypeDeleteDto dto)
    {
        var t = await db.CommunicationTypes.FindAsync(dto.TypeCode)
             ?? throw new KeyNotFoundException($"Type '{dto.TypeCode}' not found.");
        db.CommunicationTypes.Remove(t);
        await db.SaveChangesAsync();
    }
}
