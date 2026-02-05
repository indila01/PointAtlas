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

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MarkerDto>> GetMarker(Guid id)
    {
        var result = await _markerService.GetMarkerByIdAsync(id);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return Ok(result.Value);
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

        var result = await _markerService.CreateMarkerAsync(request, userId);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetMarker), new { id = result.Value!.Id }, result.Value);
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

        var result = await _markerService.UpdateMarkerAsync(id, request, userId);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return Ok(result.Value);
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

        var result = await _markerService.DeleteMarkerAsync(id, userId);

        if (result.IsFailure)
        {
            return StatusCode(result.StatusCode, new { message = result.Error });
        }

        return NoContent();
    }
}
