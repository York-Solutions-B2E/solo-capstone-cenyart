using Shared.Dtos;

namespace Shared.Interfaces;

public interface ICommService
{
    Task<PaginatedResult<CommDto>> GetCommunicationsAsync(int pageNumber, int pageSize);

    Task<CommDetailsDto?> GetCommunicationByIdAsync(Guid id);

    Task<Guid> CreateCommunicationAsync(CreateCommPayload payload);
}

public interface ITypeService
{
    Task<List<TypeDto>> GetAllTypesAsync();

    Task<TypeDetailsDto?> GetTypeByCodeAsync(string typeCode);

    Task CreateTypeAsync(CreateTypePayload payload);

    Task UpdateTypeAsync(UpdateTypePayload payload);

    Task SoftDeleteTypeAsync(DeleteTypePayload payload);

    /// <summary>
    /// Validate if all given statuses are allowed for the specified type
    /// </summary>
    Task<bool> ValidateStatusesForTypeAsync(string typeCode, List<string> statusCodes);
}

public interface IGlobalStatusService
{
    Task<List<GlobalStatusDto>> GetAllGlobalStatusesAsync();
}