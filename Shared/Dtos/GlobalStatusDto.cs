using Shared.Enums;

namespace Shared.Dtos;

public class GlobalStatusDto
{
    public string StatusCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public StatusPhase Phase { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
}
