using Shared.Enums;

namespace WebApi.Data;

public class GlobalStatus
{
    public string StatusCode { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public StatusPhase Phase { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CommunicationTypeStatus> CommunicationTypeStatuses { get; set; } = [];
    public ICollection<CommunicationStatusHistory> StatusHistories { get; set; } = [];
}

public class CommunicationType
{
    public string TypeCode { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CommunicationTypeStatus> ValidStatuses { get; set; } = [];
    public ICollection<Communication> Communications { get; set; } = [];
}

public class CommunicationTypeStatus
{
    public Guid Id { get; set; }
    public string TypeCode { get; set; } = "";
    public string StatusCode { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public CommunicationType CommunicationType { get; set; } = null!;
    public GlobalStatus Status { get; set; } = null!;
}

public class Communication
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public string CurrentStatus { get; set; } = "";
    public DateTime LastUpdatedUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public CommunicationType CommunicationType { get; set; } = null!;
    public GlobalStatus CurrentStatusNavigation { get; set; } = null!;
    public ICollection<CommunicationStatusHistory> StatusHistory { get; set; } = [];
}

public class CommunicationStatusHistory
{
    public Guid Id { get; set; }
    public Guid CommunicationId { get; set; }
    public string StatusCode { get; set; } = "";
    public DateTime OccurredUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public Communication Communication { get; set; } = null!;
    public GlobalStatus Status { get; set; } = null!;
}
