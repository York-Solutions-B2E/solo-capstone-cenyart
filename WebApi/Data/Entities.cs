
namespace WebApi.Data;

public class CommunicationType
{
    public required string TypeCode { get; set; }

    public required string DisplayName { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CommunicationTypeStatus> ValidStatuses { get; set; } = [];
    public ICollection<Communication> Communications { get; set; } = [];
}

public class CommunicationTypeStatus
{
    public Guid Id { get; set; }

    public required string TypeCode { get; set; }
    public required string StatusCode { get; set; }
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;

    public CommunicationType CommunicationType { get; set; } = null!;
}

public class Communication
{
    public Guid Id { get; set; }

    public required string Title { get; set; }
    public required string TypeCode { get; set; }
    public required string CurrentStatus { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public CommunicationType CommunicationType { get; set; } = null!;
    public ICollection<CommunicationStatusHistory> StatusHistory { get; set; } = [];
}

public class CommunicationStatusHistory
{
    public Guid Id { get; set; }

    public Guid CommunicationId { get; set; }
    public required string StatusCode { get; set; }
    public DateTime OccurredUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public Communication Communication { get; set; } = null!;
}
