
namespace Shared.DTOs;

public class EventDto
{
    public Guid CommunicationId { get; set; }
    public string EventType { get; set; } = "";
    public DateTime TimestampUtc { get; set; }
}
