using WebApi.Data.Entities;

namespace WebApi.Data.Repositories.Interfaces;

public interface ICommunicationTypeRepository : IRepository<CommunicationType>
{
    Task<List<CommunicationType>> GetAllActiveAsync();
    Task<CommunicationType?> GetByCodeWithStatusesAsync(string typeCode);
    Task<bool> HasActiveCommunicationsAsync(string typeCode);
}
