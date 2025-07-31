
using Shared.Dtos;

namespace Shared.Interfaces;

public interface IStatusTaxonomyService
{
    Task<List<GlobalStatusDto>> GetAllGlobalStatusesAsync();
    Task<List<GlobalStatusDto>> GetValidStatusesForTypeAsync(string typeCode);
    Task ValidateStatusTransitionAsync(string typeCode, string fromStatus, string toStatus);
    Task UpdateTypeStatusMappingsAsync(string typeCode, List<string> statusCodes);
}
