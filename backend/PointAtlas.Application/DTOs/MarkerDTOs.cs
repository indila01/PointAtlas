namespace PointAtlas.Application.DTOs;

public record MarkerDto(
    Guid Id,
    string Title,
    string? Description,
    double Latitude,
    double Longitude,
    string Category,
    Dictionary<string, object>? Properties,
    string CreatedById,
    string CreatedByDisplayName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateMarkerRequest(
    string Title,
    string? Description,
    double Latitude,
    double Longitude,
    string Category,
    Dictionary<string, object>? Properties
);

public record UpdateMarkerRequest(
    string Title,
    string? Description,
    double Latitude,
    double Longitude,
    string Category,
    Dictionary<string, object>? Properties
);

public record MarkerFilterDto(
    string? Category,
    string? Search,
    double? MinLatitude,
    double? MaxLatitude,
    double? MinLongitude,
    double? MaxLongitude,
    int Page = 1,
    int PageSize = 100
);

public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
