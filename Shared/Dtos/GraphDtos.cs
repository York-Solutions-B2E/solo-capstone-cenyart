
namespace Shared.Dtos;

public class CommGraphDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string TypeCode { get; set; } = default!;
    public string CurrentStatusCode { get; set; } = default!;
    public DateTime LastUpdatedUtc { get; set; }
}
