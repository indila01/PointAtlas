using AutoMapper;
using FluentAssertions;
using Moq;
using NetTopologySuite.Geometries;
using PointAtlas.Application.DTOs;
using PointAtlas.Application.Mappings;
using PointAtlas.Application.Services.Implementations;
using PointAtlas.Core.Entities;
using PointAtlas.Core.Interfaces;
using Xunit;

namespace PointAtlas.Application.Tests.Services;

public class MarkerServiceTests
{
    private readonly Mock<IMarkerRepository> _markerRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;
    private readonly MarkerService _markerService;

    public MarkerServiceTests()
    {
        _markerRepositoryMock = new Mock<IMarkerRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        // Configure AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MarkerProfile>());
        _mapper = config.CreateMapper();

        _markerService = new MarkerService(
            _markerRepositoryMock.Object,
            _mapper,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task GetMarkersAsync_ShouldReturnSuccess()
    {
        // Arrange
        var filters = new MarkerFilterDto(null, null, null, null, null, null, 1, 10);
        var markers = new List<Marker>
        {
            CreateTestMarker("marker1", "Test Marker 1", 40.7128, -74.0060),
            CreateTestMarker("marker2", "Test Marker 2", 40.7580, -73.9855)
        };

        _markerRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(markers);

        // Act
        var result = await _markerService.GetMarkersAsync(filters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetMarkersAsync_WithCategoryFilter_ShouldFilterResults()
    {
        // Arrange
        var filters = new MarkerFilterDto("Restaurant", null, null, null, null, null, 1, 10);
        var markers = new List<Marker>
        {
            CreateTestMarker("marker1", "Restaurant 1", 40.7128, -74.0060, "Restaurant"),
            CreateTestMarker("marker2", "Park 1", 40.7580, -73.9855, "Park"),
            CreateTestMarker("marker3", "Restaurant 2", 40.7489, -73.9680, "Restaurant")
        };

        _markerRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(markers);

        // Act
        var result = await _markerService.GetMarkersAsync(filters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(m => m.Category.Should().Be("Restaurant"));
    }

    [Fact]
    public async Task GetMarkersAsync_WithSearchFilter_ShouldFilterResults()
    {
        // Arrange
        var filters = new MarkerFilterDto(null, "pizza", null, null, null, null, 1, 10);
        var markers = new List<Marker>
        {
            CreateTestMarker("marker1", "Pizza Place", 40.7128, -74.0060, description: "Best pizza in town"),
            CreateTestMarker("marker2", "Burger Joint", 40.7580, -73.9855, description: "Gourmet burgers"),
            CreateTestMarker("marker3", "Italian Pizza", 40.7489, -73.9680, description: "Authentic Italian")
        };

        _markerRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(markers);

        // Act
        var result = await _markerService.GetMarkersAsync(filters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMarkersAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var filters = new MarkerFilterDto(null, null, null, null, null, null, 2, 2);
        var markers = new List<Marker>
        {
            CreateTestMarker("marker1", "Marker 1", 40.7128, -74.0060),
            CreateTestMarker("marker2", "Marker 2", 40.7580, -73.9855),
            CreateTestMarker("marker3", "Marker 3", 40.7489, -73.9680),
            CreateTestMarker("marker4", "Marker 4", 40.7614, -73.9776)
        };

        _markerRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(markers);

        // Act
        var result = await _markerService.GetMarkersAsync(filters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task GetMarkerByIdAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var marker = CreateTestMarker(markerId.ToString(), "Test Marker", 40.7128, -74.0060);

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(marker);

        // Act
        var result = await _markerService.GetMarkerByIdAsync(markerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(markerId);
        result.Value.Title.Should().Be("Test Marker");
    }

    [Fact]
    public async Task GetMarkerByIdAsync_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var markerId = Guid.NewGuid();

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync((Marker)null);

        // Act
        var result = await _markerService.GetMarkerByIdAsync(markerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task CreateMarkerAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var userId = "user123";
        var request = new CreateMarkerRequest(
            "New Marker",
            "Test description",
            40.7128,
            -74.0060,
            "Restaurant",
            new Dictionary<string, object> { { "rating", "5" } });

        var createdMarker = CreateTestMarker(Guid.NewGuid().ToString(), request.Title, request.Latitude, request.Longitude);

        _markerRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Marker>()))
            .ReturnsAsync(createdMarker);

        // Act
        var result = await _markerService.CreateMarkerAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(request.Title);
        result.Value.Latitude.Should().Be(request.Latitude);
        result.Value.Longitude.Should().Be(request.Longitude);
        result.Value.Category.Should().Be(request.Category);
    }

    [Fact]
    public async Task UpdateMarkerAsync_AsOwner_ShouldReturnSuccess()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";
        var request = new UpdateMarkerRequest(
            "Updated Marker",
            "Updated description",
            40.7128,
            -74.0060,
            "Museum",
            null);

        var existingMarker = CreateTestMarker(markerId.ToString(), "Old Title", 40.7128, -74.0060);
        existingMarker.CreatedById = userId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(existingMarker);

        _currentUserServiceMock.Setup(x => x.IsInRole("Admin"))
            .Returns(false);

        _markerRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Marker>()))
            .ReturnsAsync((Marker m) => m);

        // Act
        var result = await _markerService.UpdateMarkerAsync(markerId, request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(request.Title);
        result.Value.Category.Should().Be(request.Category);
    }

    [Fact]
    public async Task UpdateMarkerAsync_AsAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "admin123";
        var ownerId = "owner123";
        var request = new UpdateMarkerRequest(
            "Admin Updated",
            "Admin description",
            40.7128,
            -74.0060,
            "Landmark",
            null);

        var existingMarker = CreateTestMarker(markerId.ToString(), "Old Title", 40.7128, -74.0060);
        existingMarker.CreatedById = ownerId; // Different from userId

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(existingMarker);

        _currentUserServiceMock.Setup(x => x.IsInRole("Admin"))
            .Returns(true);

        _markerRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Marker>()))
            .ReturnsAsync((Marker m) => m);

        // Act
        var result = await _markerService.UpdateMarkerAsync(markerId, request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be(request.Title);
    }

    [Fact]
    public async Task UpdateMarkerAsync_AsNonOwnerNonAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";
        var ownerId = "owner456";
        var request = new UpdateMarkerRequest("Updated", null, 40.7128, -74.0060, "Park", null);

        var existingMarker = CreateTestMarker(markerId.ToString(), "Old Title", 40.7128, -74.0060);
        existingMarker.CreatedById = ownerId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(existingMarker);

        _currentUserServiceMock.Setup(x => x.IsInRole("Admin"))
            .Returns(false);

        // Act
        var result = await _markerService.UpdateMarkerAsync(markerId, request, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403); // Forbidden
        result.Error.Should().Contain("only edit your own");
    }

    [Fact]
    public async Task UpdateMarkerAsync_WithNonExistentMarker_ShouldReturnNotFound()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";
        var request = new UpdateMarkerRequest("Updated", null, 40.7128, -74.0060, "Park", null);

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync((Marker)null);

        // Act
        var result = await _markerService.UpdateMarkerAsync(markerId, request, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteMarkerAsync_AsOwner_ShouldReturnSuccess()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";

        var existingMarker = CreateTestMarker(markerId.ToString(), "Test", 40.7128, -74.0060);
        existingMarker.CreatedById = userId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(existingMarker);

        _currentUserServiceMock.Setup(x => x.IsInRole("Admin"))
            .Returns(false);

        _markerRepositoryMock.Setup(x => x.DeleteAsync(markerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _markerService.DeleteMarkerAsync(markerId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _markerRepositoryMock.Verify(x => x.DeleteAsync(markerId), Times.Once);
    }

    [Fact]
    public async Task DeleteMarkerAsync_AsAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "admin123";
        var ownerId = "owner123";

        var existingMarker = CreateTestMarker(markerId.ToString(), "Test", 40.7128, -74.0060);
        existingMarker.CreatedById = ownerId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(existingMarker);

        _currentUserServiceMock.Setup(x => x.IsInRole("Admin"))
            .Returns(true);

        _markerRepositoryMock.Setup(x => x.DeleteAsync(markerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _markerService.DeleteMarkerAsync(markerId, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _markerRepositoryMock.Verify(x => x.DeleteAsync(markerId), Times.Once);
    }

    [Fact]
    public async Task DeleteMarkerAsync_AsNonOwnerNonAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";
        var ownerId = "owner456";

        var existingMarker = CreateTestMarker(markerId.ToString(), "Test", 40.7128, -74.0060);
        existingMarker.CreatedById = ownerId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(existingMarker);

        _currentUserServiceMock.Setup(x => x.IsInRole("Admin"))
            .Returns(false);

        // Act
        var result = await _markerService.DeleteMarkerAsync(markerId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(403);
        result.Error.Should().Contain("only delete your own");
    }

    [Fact]
    public async Task DeleteMarkerAsync_WithNonExistentMarker_ShouldReturnNotFound()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync((Marker)null);

        // Act
        var result = await _markerService.DeleteMarkerAsync(markerId, userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task CanUserModifyMarkerAsync_AsAdmin_ShouldReturnTrue()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "admin123";
        var isAdmin = true;

        // Act
        var result = await _markerService.CanUserModifyMarkerAsync(markerId, userId, isAdmin);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserModifyMarkerAsync_AsOwner_ShouldReturnTrue()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";
        var isAdmin = false;

        var marker = CreateTestMarker(markerId.ToString(), "Test", 40.7128, -74.0060);
        marker.CreatedById = userId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(marker);

        // Act
        var result = await _markerService.CanUserModifyMarkerAsync(markerId, userId, isAdmin);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserModifyMarkerAsync_AsNonOwnerNonAdmin_ShouldReturnFalse()
    {
        // Arrange
        var markerId = Guid.NewGuid();
        var userId = "user123";
        var ownerId = "owner456";
        var isAdmin = false;

        var marker = CreateTestMarker(markerId.ToString(), "Test", 40.7128, -74.0060);
        marker.CreatedById = ownerId;

        _markerRepositoryMock.Setup(x => x.GetByIdAsync(markerId))
            .ReturnsAsync(marker);

        // Act
        var result = await _markerService.CanUserModifyMarkerAsync(markerId, userId, isAdmin);

        // Assert
        result.Should().BeFalse();
    }

    private Marker CreateTestMarker(string id, string title, double lat, double lng, string category = "Restaurant", string? description = null)
    {
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        return new Marker
        {
            Id = Guid.Parse(id.Length == 36 ? id : Guid.NewGuid().ToString()),
            Title = title,
            Description = description,
            Location = geometryFactory.CreatePoint(new Coordinate(lng, lat)),
            Latitude = lat,
            Longitude = lng,
            Category = category,
            Properties = new Dictionary<string, object>(),
            CreatedById = "test_user",
            CreatedBy = new ApplicationUser { Id = "test_user", DisplayName = "Test User" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
