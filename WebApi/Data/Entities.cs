
namespace WebApi.Data;

/// <summary>
/// Canonical/global status (e.g. "Shipped"). Not deletable by business rules.
/// </summary>
public class GlobalStatus
{
    public required string StatusCode { get; set; }         // PK
    public required string Phase { get; set; }
    public required string Notes { get; set; }

    public ICollection<Status> StatusLinks { get; set; } = new List<Status>();
}

/// <summary>
/// Communication Type (EOB, EOP, IDCard...). Soft-deletable via IsActive.
/// </summary>
public class Type
{
    public required string TypeCode { get; set; }     // PK
    public required string DisplayName { get; set; }
    public bool IsActive { get; set; } = true;        // soft-delete flag

    public ICollection<Status> ValidStatuses { get; set; } = new List<Status>();
    public ICollection<Communication> Communications { get; set; } = new List<Communication>();
}

/// <summary>
/// Join record: which GlobalStatus codes are valid for a Type.
/// Soft-deletable via IsActive.
/// </summary>
public class Status
{
    public Guid Id { get; set; }                      // PK
    public required string TypeCode { get; set; }     // FK -> Type.TypeCode
    public required string StatusCode { get; set; }   // FK -> GlobalStatus.StatusCode
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;        // soft-delete flag

    public Type Type { get; set; } = null!;
    public GlobalStatus GlobalStatus { get; set; } = null!;
}

/// <summary>
/// Main communication entity. NOT deletable by DB rules (no cascade).
/// </summary>
public class Communication
{
    public Guid Id { get; set; }                      // PK
    public required string Title { get; set; }
    public required string TypeCode { get; set; }     // FK -> Type.TypeCode
    public required string CurrentStatusCode { get; set; } // FK -> GlobalStatus.StatusCode
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    public Type? Type { get; set; }     // optional to handle soft delete
    public ICollection<StatusHistory> StatusHistory { get; set; } = new List<StatusHistory>();
}

/// <summary>
/// Status history entries for a communication. NOT deletable by DB rules.
/// </summary>
public class StatusHistory
{
    public Guid Id { get; set; }                      // PK
    public Guid CommunicationId { get; set; }         // FK -> Communication.Id
    public required string StatusCode { get; set; }   // GlobalStatus.StatusCode
    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;

    public Communication Communication { get; set; } = null!;
}
