using Shared.DTOs;

namespace Shared.Interfaces;

public interface ICommunicationService
{
    Task<List<CommunicationDto>> GetAllAsync();
    Task<CommunicationDto> GetByIdAsync(Guid id);
    Task<CommunicationDto> CreateAsync(CreateCommunicationDto dto);
    Task UpdateStatusAsync(Guid id, string newStatus);
    Task SoftDeleteAsync(Guid id);
}
