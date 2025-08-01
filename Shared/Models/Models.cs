
namespace Shared.Models;

public class Communication
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public string CurrentStatus { get; set; } = "";
    public DateTime LastUpdatedUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
