using AutoMapper;
using NetTopologySuite.Geometries;
using PointAtlas.Application.DTOs;
using PointAtlas.Application.Services.Interfaces;
using PointAtlas.Core.Entities;
using PointAtlas.Core.Interfaces;

namespace PointAtlas.Application.Services.Implementations;

public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _markerRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public MarkerService(
        IMarkerRepository markerRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _markerRepository = markerRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<MarkerDto>> GetMarkersAsync(MarkerFilterDto filters)
    {
        IEnumerable<Marker> markers;

        // Apply spatial filtering if bounds are provided
        if (filters.MinLatitude.HasValue && filters.MaxLatitude.HasValue &&
            filters.MinLongitude.HasValue && filters.MaxLongitude.HasValue)
        {
            markers = await _markerRepository.GetMarkersInBoundsAsync(
                filters.MinLatitude.Value,
                filters.MaxLatitude.Value,
                filters.MinLongitude.Value,
                filters.MaxLongitude.Value);
        }
        else
        {
            markers = await _markerRepository.GetAllAsync();
        }

        // Apply text filters
        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            markers = markers.Where(m =>
                m.Category.Equals(filters.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchLower = filters.Search.ToLower();
            markers = markers.Where(m =>
                m.Title.ToLower().Contains(searchLower) ||
                (m.Description != null && m.Description.ToLower().Contains(searchLower)));
        }

        var markersList = markers.ToList();
        var totalCount = markersList.Count;

        // Apply pagination
        var pagedMarkers = markersList
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        var markerDtos = _mapper.Map<List<MarkerDto>>(pagedMarkers);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);

        return new PagedResultDto<MarkerDto>(
            markerDtos,
            totalCount,
            filters.Page,
            filters.PageSize,
            totalPages);
    }

    public async Task<MarkerDto> GetMarkerByIdAsync(Guid id)
    {
        var marker = await _markerRepository.GetByIdAsync(id);
        if (marker == null)
        {
            throw new KeyNotFoundException($"Marker with ID {id} not found");
        }

        return _mapper.Map<MarkerDto>(marker);
    }

    public async Task<MarkerDto> CreateMarkerAsync(CreateMarkerRequest request, string userId)
    {
        var marker = _mapper.Map<Marker>(request);
        marker.Id = Guid.NewGuid();
        marker.CreatedById = userId;
        marker.CreatedAt = DateTime.UtcNow;
        marker.UpdatedAt = DateTime.UtcNow;

        // Create Point geometry with SRID 4326 (WGS 84)
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        marker.Location = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));

        var createdMarker = await _markerRepository.CreateAsync(marker);
        return _mapper.Map<MarkerDto>(createdMarker);
    }

    public async Task<MarkerDto> UpdateMarkerAsync(Guid id, UpdateMarkerRequest request, string userId)
    {
        var marker = await _markerRepository.GetByIdAsync(id);
        if (marker == null)
        {
            throw new KeyNotFoundException($"Marker with ID {id} not found");
        }

        // Authorization check
        bool isAdmin = _currentUserService.IsInRole("Admin");
        if (marker.CreatedById != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You can only edit your own markers");
        }

        // Update properties
        marker.Title = request.Title;
        marker.Description = request.Description;
        marker.Category = request.Category;
        marker.Latitude = request.Latitude;
        marker.Longitude = request.Longitude;
        marker.Properties = request.Properties;
        marker.UpdatedAt = DateTime.UtcNow;

        // Update Point geometry
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        marker.Location = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));

        var updatedMarker = await _markerRepository.UpdateAsync(marker);
        return _mapper.Map<MarkerDto>(updatedMarker);
    }

    public async Task DeleteMarkerAsync(Guid id, string userId)
    {
        var marker = await _markerRepository.GetByIdAsync(id);
        if (marker == null)
        {
            throw new KeyNotFoundException($"Marker with ID {id} not found");
        }

        // Authorization check
        bool isAdmin = _currentUserService.IsInRole("Admin");
        if (marker.CreatedById != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You can only delete your own markers");
        }

        await _markerRepository.DeleteAsync(id);
    }

    public async Task<bool> CanUserModifyMarkerAsync(Guid markerId, string userId, bool isAdmin)
    {
        if (isAdmin)
        {
            return true;
        }

        var marker = await _markerRepository.GetByIdAsync(markerId);
        return marker != null && marker.CreatedById == userId;
    }
}
