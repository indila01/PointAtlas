using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PointAtlas.Core.Entities;
using PointAtlas.Core.Interfaces;
using PointAtlas.Infrastructure.Data;

namespace PointAtlas.Infrastructure.Repositories;

public class MarkerRepository : IMarkerRepository
{
    private readonly PointAtlasDbContext _context;

    public MarkerRepository(PointAtlasDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Marker>> GetAllAsync()
    {
        return await _context.Markers
            .Include(m => m.CreatedBy)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Marker?> GetByIdAsync(Guid id)
    {
        return await _context.Markers
            .Include(m => m.CreatedBy)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Marker> CreateAsync(Marker marker)
    {
        _context.Markers.Add(marker);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(marker)
            .Reference(m => m.CreatedBy)
            .LoadAsync();

        return marker;
    }

    public async Task<Marker> UpdateAsync(Marker marker)
    {
        marker.UpdatedAt = DateTime.UtcNow;
        _context.Markers.Update(marker);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(marker)
            .Reference(m => m.CreatedBy)
            .LoadAsync();

        return marker;
    }

    public async Task DeleteAsync(Guid id)
    {
        var marker = await _context.Markers.FindAsync(id);
        if (marker != null)
        {
            _context.Markers.Remove(marker);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Marker>> GetMarkersInBoundsAsync(
        double minLat, double maxLat, double minLng, double maxLng)
    {
        // Use coordinate range checks instead of ST_Contains for geography compatibility
        // This is more efficient and works with both geometry and geography types
        return await _context.Markers
            .Where(m => m.Latitude >= minLat
                     && m.Latitude <= maxLat
                     && m.Longitude >= minLng
                     && m.Longitude <= maxLng)
            .Include(m => m.CreatedBy)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Marker>> GetMarkersNearbyAsync(double lat, double lng, double radiusKm)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var point = geometryFactory.CreatePoint(new Coordinate(lng, lat));
        var radiusMeters = radiusKm * 1000;

        return await _context.Markers
            .Where(m => m.Location.Distance(point) <= radiusMeters)
            .Include(m => m.CreatedBy)
            .OrderBy(m => m.Location.Distance(point))
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Markers.CountAsync();
    }
}
