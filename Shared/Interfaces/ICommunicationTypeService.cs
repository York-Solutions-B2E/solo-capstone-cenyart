using Shared.Dtos;

namespace Shared.Interfaces;

public interface ICommunicationTypeService
{
    Task<List<CommunicationTypeDto>> GetAllActiveAsync();
    Task<List<CommunicationTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<CommunicationTypeDto?> GetByCodeAsync(string typeCode);
    Task<CommunicationTypeDto> CreateAsync(CreateCommunicationDto request);
    Task<CommunicationTypeDto> UpdateAsync(string typeCode, UpdateCommunicationDto request);
    Task SoftDeleteAsync(string typeCode);
    Task RestoreAsync(string typeCode);
}
