namespace Shared.Dtos;

// ----------------------
// Global Status
// ----------------------
public sealed record GlobalStatusDto(
    string StatusCode,
    string Phase,
    string Notes
);

// ----------------------
// Lightweight communication list‐view
// ----------------------
public sealed record CommDto(
    Guid Id,
    string Title,
    string TypeCode,
    string CurrentStatusCode,
    DateTime LastUpdatedUtc
);

// ----------------------
// Communication detail + history
// ----------------------
public sealed record CommDetailsDto(
    Guid Id,
    string Title,
    string TypeCode,
    string CurrentStatusCode,
    DateTime LastUpdatedUtc,
    List<StatusHistoryDto> StatusHistory
);

// ----------------------
// Communication history item
// ----------------------
public sealed record StatusHistoryDto(
    string StatusCode,
    DateTime OccurredUtc
);

// ----------------------
// Type list‐view
// ----------------------
public sealed record TypeDto(
    string TypeCode,
    string DisplayName,
    bool IsActive
);

// ----------------------
// Type + its valid statuses
// ----------------------
public sealed record TypeDetailsDto(
    string TypeCode,
    string DisplayName,
    bool IsActive,
    List<StatusDto> ValidStatuses
);

// ----------------------
// A single status under a type
// ----------------------
public sealed record StatusDto(
    Guid Id,
    string TypeCode,
    string StatusCode,
    string Description,
    bool IsActive
);
