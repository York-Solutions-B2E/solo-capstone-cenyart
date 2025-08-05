
namespace Shared.DTOs;

public class EventDto
{
    public Guid CommunicationId { get; set; }
    public required string StatusCode { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}

