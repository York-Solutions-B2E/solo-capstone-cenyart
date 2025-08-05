using Shared.DTOs;

namespace Shared.Interfaces;
public interface IStatusService
{
    /// <summary>
    /// Get all valid statuses for a given communication type.
    /// </summary>
    Task<List<StatusOptionDto>> GetForTypeAsync(string typeCode);
}
