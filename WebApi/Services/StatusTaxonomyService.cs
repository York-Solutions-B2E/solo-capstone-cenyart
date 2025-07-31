using Microsoft.EntityFrameworkCore;
using Shared.Dtos;
using Shared.Exceptions;
using Shared.Interfaces;
using WebApi.Data;
using WebApi.Data.Entities;

namespace WebApi.Services;

public class StatusTaxonomyService(CommunicationDbContext context, ILogger<StatusTaxonomyService> logger) : IStatusTaxonomyService
{
    private readonly CommunicationDbContext _context = context;
    private readonly ILogger<StatusTaxonomyService> _logger = logger;

    public async Task<List<GlobalStatusDto>> GetAllGlobalStatusesAsync()
    {
        var entities = await _context.GlobalStatuses
            .Where(gs => gs.IsActive)
            .OrderBy(gs => gs.Phase)
            .ThenBy(gs => gs.SortOrder)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<GlobalStatusDto>> GetValidStatusesForTypeAsync(string typeCode)
    {
        var entities = await _context.CommunicationTypeStatuses
            .Where(cts => cts.TypeCode == typeCode && cts.IsActive)
            .Include(cts => cts.GlobalStatus)
            .Select(cts => cts.GlobalStatus)
            .OrderBy(gs => gs.Phase)
            .ThenBy(gs => gs.SortOrder)
            .ToListAsync();

        return entities.Select(MapToDto).ToList();
    }

    public async Task ValidateStatusTransitionAsync(string typeCode, string fromStatus, string toStatus)
    {
        var validStatuses = await GetValidStatusesForTypeAsync(typeCode);
        var validStatusCodes = validStatuses.Select(vs => vs.StatusCode).ToList();

        if (!validStatusCodes.Contains(fromStatus))
            throw new BusinessRuleException($"Status '{fromStatus}' is not valid for communication type '{typeCode}'");

        if (!validStatusCodes.Contains(toStatus))
            throw new BusinessRuleException($"Status '{toStatus}' is not valid for communication type '{typeCode}'");

        // Additional business rules for status transitions can be added here
        // For example, preventing backwards transitions, etc.
    }

    public async Task UpdateTypeStatusMappingsAsync(string typeCode, List<string> statusCodes)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Remove existing mappings
            var existingMappings = await _context.CommunicationTypeStatuses
                .Where(cts => cts.TypeCode == typeCode)
                .ToListAsync();

            _context.CommunicationTypeStatuses.RemoveRange(existingMappings);

            // Add new mappings
            var newMappings = statusCodes.Select((statusCode, index) => new CommunicationTypeStatus
            {
                TypeCode = typeCode,
                StatusCode = statusCode,
                SortOrder = index + 1,
                IsActive = true,
                Description = $"{statusCode} status for {typeCode}"
            }).ToList();

            await _context.CommunicationTypeStatuses.AddRangeAsync(newMappings);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Updated status mappings for type {TypeCode}: {StatusCodes}",
                typeCode, string.Join(", ", statusCodes));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static GlobalStatusDto MapToDto(GlobalStatus entity)
    {
        return new GlobalStatusDto
        {
            StatusCode = entity.StatusCode,
            DisplayName = entity.DisplayName,
            Phase = entity.Phase,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            CreatedUtc = entity.CreatedUtc
        };
    }
}
