using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointAtlas.Application.DTOs;
using PointAtlas.Application.Services.Interfaces;

namespace PointAtlas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarkersController : ControllerBase
{
    private readonly IMarkerService _markerService;

    public MarkersController(IMarkerService markerService)
    {
        _markerService = markerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<MarkerDto>>> GetMarkers(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] double? minLatitude,
        [FromQuery] double? maxLatitude,
        [FromQuery] double? minLongitude,
        [FromQuery] double? maxLongitude,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var filters = new MarkerFilterDto(
            category,
            search,
            minLatitude,
            maxLatitude,
            minLongitude,
            maxLongitude,
            page,
            pageSize);

        var result = await _markerService.GetMarkersAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MarkerDto>> GetMarker(Guid id)
    {
        try
        {
            var marker = await _markerService.GetMarkerByIdAsync(id);
            return Ok(marker);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MarkerDto>> CreateMarker([FromBody] CreateMarkerRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var marker = await _markerService.CreateMarkerAsync(request, userId);
        return CreatedAtAction(nameof(GetMarker), new { id = marker.Id }, marker);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<MarkerDto>> UpdateMarker(Guid id, [FromBody] UpdateMarkerRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var marker = await _markerService.UpdateMarkerAsync(id, request, userId);
            return Ok(marker);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteMarker(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _markerService.DeleteMarkerAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }
}
