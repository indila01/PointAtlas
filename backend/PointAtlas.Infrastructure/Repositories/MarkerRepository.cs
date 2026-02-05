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
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

        // Create a bounding box polygon
        var coordinates = new[]
        {
            new Coordinate(minLng, minLat),
            new Coordinate(maxLng, minLat),
            new Coordinate(maxLng, maxLat),
            new Coordinate(minLng, maxLat),
            new Coordinate(minLng, minLat)
        };

        var boundingBox = geometryFactory.CreatePolygon(coordinates);

        return await _context.Markers
            .Where(m => boundingBox.Contains(m.Location))
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
