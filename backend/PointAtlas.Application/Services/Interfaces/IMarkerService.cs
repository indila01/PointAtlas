using PointAtlas.Application.DTOs;

namespace PointAtlas.Application.Services.Interfaces;

public interface IMarkerService
{
    Task<PagedResultDto<MarkerDto>> GetMarkersAsync(MarkerFilterDto filters);
    Task<MarkerDto> GetMarkerByIdAsync(Guid id);
    Task<MarkerDto> CreateMarkerAsync(CreateMarkerRequest request, string userId);
    Task<MarkerDto> UpdateMarkerAsync(Guid id, UpdateMarkerRequest request, string userId);
    Task DeleteMarkerAsync(Guid id, string userId);
    Task<bool> CanUserModifyMarkerAsync(Guid markerId, string userId, bool isAdmin);
}
