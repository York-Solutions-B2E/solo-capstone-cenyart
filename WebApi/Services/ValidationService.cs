using Shared.Interfaces;

namespace WebApi.Services;

public class ValidationService(IStatusService statusSvc) : IValidationService
{
    public async Task ValidateStatusForTypeAsync(string typeCode, string status)
    {
        var statuses = await statusSvc.GetByTypeAsync(typeCode);
        if (!statuses.Any(s => 
            string.Equals(s.StatusCode, status, StringComparison.OrdinalIgnoreCase) 
            && s.IsActive))
        {
            throw new InvalidOperationException(
                $"Status '{status}' is not valid or not active for type '{typeCode}'.");
        }
    }
}
