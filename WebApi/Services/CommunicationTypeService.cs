using Shared.Dtos;
using Shared.Exceptions;
using Shared.Interfaces;
using WebApi.Data.Entities;
using WebApi.Data.Repositories.Interfaces;
using WebApi.Rules;

namespace WebApi.Services;

public class CommunicationTypeService : ICommunicationTypeService
{
    private readonly ICommunicationTypeRepository _repository;
    private readonly IStatusTaxonomyService _statusTaxonomyService;
    private readonly ILogger<CommunicationTypeService> _logger;

    public CommunicationTypeService(
        ICommunicationTypeRepository repository,
        IStatusTaxonomyService statusTaxonomyService,
        ILogger<CommunicationTypeService> logger)
    {
        _repository = repository;
        _statusTaxonomyService = statusTaxonomyService;
        _logger = logger;
    }

    public async Task<List<CommunicationTypeDto>> GetAllActiveAsync()
    {
        var entities = await _repository.GetAllActiveAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<CommunicationTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var entities = await _repository.GetAllAsync();

        if (!includeInactive)
        {
            entities = entities.Where(ct => ct.IsActive).ToList();
        }

        return entities.OrderBy(ct => ct.DisplayName).Select(MapToDto).ToList();
    }

    public async Task<CommunicationTypeDto?> GetByCodeAsync(string typeCode)
    {
        if (string.IsNullOrWhiteSpace(typeCode))
            throw new ArgumentException("Type code cannot be null or empty", nameof(typeCode));

        var entity = await _repository.GetByCodeWithStatusesAsync(typeCode);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<CommunicationTypeDto> CreateAsync(CreateCommunicationDto request)
    {
        // Validate request
        CommunicationRules.ValidationRules.ValidateTypeCode(request.TypeCode);

        // Check if type code already exists
        var existing = await _repository.GetByIdAsync(request.TypeCode);
        if (existing != null)
            throw new BusinessRuleException($"Communication type with code '{request.TypeCode}' already exists");

        // Validate status mappings
        var globalStatuses = await _statusTaxonomyService.GetAllGlobalStatusesAsync();
        CommunicationRules.ValidationRules.ValidateStatusMappings(request.ValidStatusCodes, globalStatuses);

        var entity = new CommunicationType
        {
            TypeCode = request.TypeCode,
            DisplayName = request.DisplayName,
            Description = request.Description,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow
        };

        var createdEntity = await _repository.AddAsync(entity);

        // Add status mappings
        if (request.ValidStatusCodes?.Any() == true)
        {
            await _statusTaxonomyService.UpdateTypeStatusMappingsAsync(request.TypeCode, request.ValidStatusCodes);
        }

        _logger.LogInformation("Created communication type: {TypeCode}", request.TypeCode);

        // Reload with status mappings
        var reloadedEntity = await _repository.GetByCodeWithStatusesAsync(request.TypeCode);
        return MapToDto(reloadedEntity!);
    }

    public async Task<CommunicationTypeDto> UpdateAsync(string typeCode, UpdateCommunicationDto request)
    {
        var existingEntity = await _repository.GetByCodeWithStatusesAsync(typeCode);
        if (existingEntity == null)
            throw new NotFoundException($"Communication type '{typeCode}' not found");

        // Validate status mappings
        var globalStatuses = await _statusTaxonomyService.GetAllGlobalStatusesAsync();
        CommunicationRules.ValidationRules.ValidateStatusMappings(request.ValidStatusCodes, globalStatuses);

        existingEntity.DisplayName = request.DisplayName;
        existingEntity.Description = request.Description;
        existingEntity.ModifiedUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(existingEntity);

        // Update status mappings
        if (request.ValidStatusCodes != null)
        {
            await _statusTaxonomyService.UpdateTypeStatusMappingsAsync(typeCode, request.ValidStatusCodes);
        }

        _logger.LogInformation("Updated communication type: {TypeCode}", typeCode);

        // Reload with updated status mappings
        var updatedEntity = await _repository.GetByCodeWithStatusesAsync(typeCode);
        return MapToDto(updatedEntity!);
    }

    public async Task SoftDeleteAsync(string typeCode)
    {
        var existingEntity = await _repository.GetByIdAsync(typeCode);
        if (existingEntity == null)
            throw new NotFoundException($"Communication type '{typeCode}' not found");

        // Check if there are active communications
        var hasActiveCommunications = await _repository.HasActiveCommunicationsAsync(typeCode);
        CommunicationRules.ValidationRules.ValidateBeforeDelete(typeCode, hasActiveCommunications ? 1 : 0);

        existingEntity.IsActive = false;
        existingEntity.ModifiedUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(existingEntity);

        _logger.LogWarning("Soft deleted communication type: {TypeCode}", typeCode);
    }

    public async Task RestoreAsync(string typeCode)
    {
        var existingEntity = await _repository.GetByIdAsync(typeCode);
        if (existingEntity == null)
            throw new NotFoundException($"Communication type '{typeCode}' not found");

        existingEntity.IsActive = true;
        existingEntity.ModifiedUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(existingEntity);

        _logger.LogInformation("Restored communication type: {TypeCode}", typeCode);
    }

    private static CommunicationTypeDto MapToDto(CommunicationType entity)
    {
        return new CommunicationTypeDto
        {
            TypeCode = entity.TypeCode,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedUtc = entity.CreatedUtc,
            ModifiedUtc = entity.ModifiedUtc,
            ValidStatuses = entity.ValidStatuses?.Where(vs => vs.IsActive)
                .Select(vs => new GlobalStatusDto
                {
                    StatusCode = vs.GlobalStatus.StatusCode,
                    DisplayName = vs.GlobalStatus.DisplayName,
                    Phase = vs.GlobalStatus.Phase,
                    SortOrder = vs.GlobalStatus.SortOrder,
                    IsActive = vs.GlobalStatus.IsActive,
                    CreatedUtc = vs.GlobalStatus.CreatedUtc
                }).ToList() ?? []
        };
    }
}
