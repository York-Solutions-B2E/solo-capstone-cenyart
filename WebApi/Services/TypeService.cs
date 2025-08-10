using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Interfaces;
using WebApi.Data;

namespace WebApi.Services;

public class TypeService(CommunicationDbContext context) : ITypeService
{
    private readonly CommunicationDbContext _context = context;

    public async Task<List<TypeDto>> GetAllTypesAsync()
    {
        return await _context.Types
            .Where(t => t.IsActive)
            .Select(t => new TypeDto(
                t.TypeCode,
                t.DisplayName,
                t.IsActive
            ))
            .ToListAsync();
    }

    public async Task<TypeDetailsDto?> GetTypeByCodeAsync(string typeCode)
    {
        var type = await _context.Types
            .Include(t => t.ValidStatuses.Where(s => s.IsActive))
            .FirstOrDefaultAsync(t => t.TypeCode == typeCode && t.IsActive);

        if (type == null)
            return null;

        return new TypeDetailsDto(
            type.TypeCode,
            type.DisplayName,
            type.IsActive,
            type.ValidStatuses.Select(s => new StatusDto(
                s.Id,
                s.TypeCode,
                s.StatusCode,
                s.Description,
                s.IsActive
            )).ToList()
        );
    }

    public async Task CreateTypeAsync(CreateTypePayload payload)
    {
        var type = new Data.Type
        {
            TypeCode = payload.TypeCode,
            DisplayName = payload.DisplayName,
            IsActive = true,
            ValidStatuses = new List<Status>()
        };

        // Add allowed statuses if provided
        if (payload.AllowedStatusCodes != null && payload.AllowedStatusCodes.Count != 0)
        {
            // Get global statuses matching the codes
            var globalStatuses = await _context.GlobalStatuses
                .Where(g => payload.AllowedStatusCodes.Contains(g.StatusCode))
                .ToListAsync();

            foreach (var gs in globalStatuses)
            {
                var status = new Status
                {
                    TypeCode = type.TypeCode,
                    StatusCode = gs.StatusCode,
                    Description = gs.Notes,
                    IsActive = true,
                    GlobalStatus = gs
                };
                type.ValidStatuses.Add(status);
            }
        }

        _context.Types.Add(type);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTypeAsync(UpdateTypePayload payload)
    {
        var type = await _context.Types
            .Include(t => t.ValidStatuses)
            .FirstOrDefaultAsync(t => t.TypeCode == payload.TypeCode && t.IsActive);

        if (type == null)
            return;

        type.DisplayName = payload.DisplayName;

        // Handle adding allowed statuses
        if (payload.AddStatusCodes != null && payload.AddStatusCodes.Count != 0)
        {
            var existingStatusCodes = type.ValidStatuses
                .Where(s => s.IsActive)
                .Select(s => s.StatusCode)
                .ToHashSet();

            var addCodes = payload.AddStatusCodes
                .Where(c => !existingStatusCodes.Contains(c))
                .ToList();

            if (addCodes.Count != 0)
            {
                var globalStatuses = await _context.GlobalStatuses
                    .Where(g => addCodes.Contains(g.StatusCode))
                    .ToListAsync();

                foreach (var gs in globalStatuses)
                {
                    var newStatus = new Status
                    {
                        TypeCode = type.TypeCode,
                        StatusCode = gs.StatusCode,
                        Description = gs.Notes,
                        IsActive = true,
                        GlobalStatus = gs
                    };
                    type.ValidStatuses.Add(newStatus);
                }
            }
        }

        // Handle removing allowed statuses (soft delete)
        if (payload.RemoveStatusCodes != null && payload.RemoveStatusCodes.Count != 0)
        {
            var toRemove = type.ValidStatuses
                .Where(s => payload.RemoveStatusCodes.Contains(s.StatusCode) && s.IsActive)
                .ToList();

            foreach (var status in toRemove)
            {
                status.IsActive = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteTypeAsync(DeleteTypePayload payload)
    {
        var type = await _context.Types
            .Include(t => t.ValidStatuses)
            .FirstOrDefaultAsync(t => t.TypeCode == payload.TypeCode);

        if (type == null)
            return;

        type.IsActive = false;

        foreach (var status in type.ValidStatuses)
        {
            status.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateStatusesForTypeAsync(string typeCode, List<string> statusCodes)
    {
        var validCodes = await _context.Statuses
            .Where(s => s.TypeCode == typeCode && s.IsActive)
            .Select(s => s.StatusCode)
            .ToListAsync();

        return statusCodes.All(validCodes.Contains);
    }
}
