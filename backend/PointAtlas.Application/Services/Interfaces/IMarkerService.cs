using PointAtlas.Application.Common;
using PointAtlas.Application.DTOs;

namespace PointAtlas.Application.Services.Interfaces;

public interface IMarkerService
{
    Task<Result<PagedResultDto<MarkerDto>>> GetMarkersAsync(MarkerFilterDto filters);
    Task<Result<MarkerDto>> GetMarkerByIdAsync(Guid id);
    Task<Result<MarkerDto>> CreateMarkerAsync(CreateMarkerRequest request, string userId);
    Task<Result<MarkerDto>> UpdateMarkerAsync(Guid id, UpdateMarkerRequest request, string userId);
    Task<Result> DeleteMarkerAsync(Guid id, string userId);
    Task<bool> CanUserModifyMarkerAsync(Guid markerId, string userId, bool isAdmin);
}
