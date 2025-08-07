using Shared.Dtos;

namespace Shared.Interfaces;

public interface ICommService
{
    Task<(IEnumerable<Dto> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
    Task<IEnumerable<Dto>>        GetAllAsync();
    Task<DetailsDto>              GetByIdAsync(Guid id);
    Task CreateAsync(CommunicationCreateDto dto);
    Task UpdateAsync(CommunicationUpdateDto dto);
    Task DeleteAsync(CommunicationDeleteDto dto);
}

public interface ITypeService
{
    Task<IEnumerable<TypeDto>>    GetAllAsync();
    Task<TypeDetailsDto>          GetByCodeAsync(string typeCode);
    Task CreateAsync(TypeCreateDto dto);
    Task UpdateAsync(TypeUpdateDto dto);
    Task DeleteAsync(TypeDeleteDto dto);
}

public interface IStatusService
{
    Task<IEnumerable<StatusDto>>  GetByTypeAsync(string typeCode);
    Task AddAsync(StatusCreateDto dto);
    Task UpdateAsync(StatusUpdateDto dto);
    Task DeleteAsync(StatusDeleteDto dto);
}

public interface IValidationService
{
    Task ValidateStatusForTypeAsync(string typeCode, string status);
}
