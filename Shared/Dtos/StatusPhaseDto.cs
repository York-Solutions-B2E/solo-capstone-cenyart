using Shared.Enums;

namespace Shared.Dtos;

public class StatusPhaseDto
{
    public StatusPhase Phase { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
