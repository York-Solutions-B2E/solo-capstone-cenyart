using Shared.Enums;

namespace WebApi.Data.Entities;

public class GlobalStatus
{
    public string StatusCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public StatusPhase Phase { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public class CommunicationType
{
    public string TypeCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ModifiedUtc { get; set; }

    // Navigation properties
    public virtual ICollection<CommunicationTypeStatus> ValidStatuses { get; set; } = [];
}

public class CommunicationTypeStatus
{
    public int Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual CommunicationType CommunicationType { get; set; } = null!;
    public virtual GlobalStatus GlobalStatus { get; set; } = null!;
}

public class Communication
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string? SourceFileUrl { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }

    // Navigation properties
    public virtual CommunicationType CommunicationType { get; set; } = null!;
    public virtual GlobalStatus CurrentStatusNavigation { get; set; } = null!;
    public virtual ICollection<CommunicationStatusHistory> StatusHistory { get; set; } = [];
}

public class CommunicationStatusHistory
{
    public long Id { get; set; }
    public Guid CommunicationId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public DateTime OccurredUtc { get; set; }
    public string? EventData { get; set; }

    // Navigation properties
    public virtual Communication Communication { get; set; } = null!;
    public virtual GlobalStatus Status { get; set; } = null!;
}
