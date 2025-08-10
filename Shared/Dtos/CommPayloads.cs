namespace Shared.Dtos;

// ----------------------
// Communication payloads
// ----------------------
public sealed record CreateCommPayload(
    string Title,
    string TypeCode,
    string CurrentStatusCode
);

// ----------------------
// Type payloads
// ----------------------
public sealed record CreateTypePayload(
    string TypeCode,
    string DisplayName,
    List<string>? AllowedStatusCodes
);

public sealed record UpdateTypePayload(
    string TypeCode,
    string DisplayName,
    List<string>? AddStatusCodes,
    List<string>? RemoveStatusCodes
);

public sealed record DeleteTypePayload(
    string TypeCode
);

public sealed record ValidateStatusesPayload(
    string TypeCode,
    List<string> StatusCodes
);

// Helper class for paginated results
public sealed record PaginatedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);