
using System.Text.RegularExpressions;
using Shared.Dtos;
using Shared.Exceptions;

namespace WebApi.Rules;

public partial class CommunicationRules
{
    public static partial class ValidationRules
    {
        public static void ValidateTypeCode(string typeCode)
        {
            if (string.IsNullOrWhiteSpace(typeCode))
                throw new BusinessRuleException("Type code is required");

            if (typeCode.Length > 20)
                throw new BusinessRuleException("Type code cannot exceed 20 characters");

            if (!MyRegex().IsMatch(typeCode))
                throw new BusinessRuleException("Type code can only contain uppercase letters, numbers, and underscores");
        }

        public static void ValidateStatusMappings(List<string> statusCodes, List<GlobalStatusDto> globalStatuses)
        {
            if (!statusCodes.Any())
                throw new BusinessRuleException("Communication type must have at least one valid status");

            var invalidStatuses = statusCodes.Except(globalStatuses.Where(gs => gs.IsActive).Select(gs => gs.StatusCode));
            if (invalidStatuses.Any())
                throw new BusinessRuleException($"Invalid status codes: {string.Join(", ", invalidStatuses)}");
        }

        public static void ValidateBeforeDelete(string typeCode, int activeCommunitationsCount)
        {
            if (activeCommunitationsCount > 0)
                throw new BusinessRuleException($"Cannot deactivate type '{typeCode}' because it has {activeCommunitationsCount} active communications");
        }

        [GeneratedRegex("^[A-Z0-9_]+$")]
        private static partial Regex MyRegex();
    }
}
