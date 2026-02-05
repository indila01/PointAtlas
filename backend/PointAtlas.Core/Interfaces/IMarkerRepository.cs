using PointAtlas.Core.Entities;

namespace PointAtlas.Core.Interfaces;

public interface IMarkerRepository
{
    Task<IEnumerable<Marker>> GetAllAsync();
    Task<Marker?> GetByIdAsync(Guid id);
    Task<Marker> CreateAsync(Marker marker);
    Task<Marker> UpdateAsync(Marker marker);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Marker>> GetMarkersInBoundsAsync(double minLat, double maxLat, double minLng, double maxLng);
    Task<IEnumerable<Marker>> GetMarkersNearbyAsync(double lat, double lng, double radiusKm);
    Task<int> CountAsync();
}
