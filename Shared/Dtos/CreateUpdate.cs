namespace Shared.Dtos;

// ======================================
// Communication Create / Update / Delete
// ======================================
public sealed record CommunicationCreateDto(
    string TypeCode,
    string Title
);

public sealed record CommunicationUpdateDto(
    Guid   Id,
    string NewStatus
);

public sealed record CommunicationDeleteDto(
    Guid Id
);

// ======================================
// CommunicationType Create / Update / Delete
// ======================================
public sealed record TypeCreateDto(
    string TypeCode,
    string DisplayName
);

public sealed record TypeUpdateDto(
    string TypeCode,
    string DisplayName,
    bool   IsActive
);

public sealed record TypeDeleteDto(
    string TypeCode
);

// ======================================
// CommunicationTypeStatus Create / Update / Delete
// ======================================
public sealed record StatusCreateDto(
    string TypeCode,
    string StatusCode,
    string Description
);

public sealed record StatusUpdateDto(
    Guid   Id,
    string StatusCode,
    string Description,
    bool   IsActive
);

public sealed record StatusDeleteDto(
    Guid Id
);
