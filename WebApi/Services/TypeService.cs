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

        // The full list user wants
        var newStatusCodes = payload.AllowedStatusCodes ?? new List<string>();

        var existingStatuses = type.ValidStatuses.Where(s => s.IsActive).ToList();
        var existingStatusCodes = existingStatuses.Select(s => s.StatusCode).ToHashSet();

        // Calculate add and remove sets
        var toAddCodes = newStatusCodes.Except(existingStatusCodes).ToList();
        var toRemoveStatuses = existingStatuses.Where(s => !newStatusCodes.Contains(s.StatusCode)).ToList();

        // Add new allowed statuses
        if (toAddCodes.Count > 0)
        {
            var globalStatuses = await _context.GlobalStatuses
                .Where(g => toAddCodes.Contains(g.StatusCode))
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

        // Soft-delete removed allowed statuses
        foreach (var status in toRemoveStatuses)
        {
            status.IsActive = false;
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
