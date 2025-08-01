
namespace Shared.DTOs;

public class StatusHistoryDto
{
    public Guid Id { get; set; }
    public string StatusCode { get; set; } = "";
    public DateTime OccurredUtc { get; set; }
}
