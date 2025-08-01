
namespace Shared.DTOs;

public class CommunicationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public string CurrentStatus { get; set; } = "";
    public DateTime LastUpdatedUtc { get; set; }
    public List<StatusHistoryDto> History { get; set; } = [];
}
