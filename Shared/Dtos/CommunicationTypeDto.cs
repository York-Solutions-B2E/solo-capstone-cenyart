
namespace Shared.Dtos;

public class CommunicationTypeDto
{
    public string TypeCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ModifiedUtc { get; set; }
    public List<GlobalStatusDto> ValidStatuses { get; set; } = new();
}
