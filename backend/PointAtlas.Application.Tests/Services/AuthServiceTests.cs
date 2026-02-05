using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using PointAtlas.Application.DTOs;
using PointAtlas.Application.Services.Implementations;
using PointAtlas.Core.Entities;
using PointAtlas.Core.Interfaces;
using Xunit;

namespace PointAtlas.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Mock UserManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123!", "Test User");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
            .Returns("access_token");

        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken());

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        result.Value.User.Email.Should().Be(request.Email);
        result.Value.User.DisplayName.Should().Be(request.DisplayName);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterRequest("existing@example.com", "Password123!", "Test User");
        var existingUser = new ApplicationUser { Email = request.Email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(409); // Conflict
        result.Error.Should().Be("User with this email already exists");
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "weak", "Test User");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser)null);

        var errors = new[] { new IdentityError { Description = "Password too weak" } };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(400);
        result.Error.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = "user_id",
            Email = request.Email,
            DisplayName = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("access_token");

        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken());

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(401); // Unauthorized
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "WrongPassword");
        var user = new ApplicationUser { Email = request.Email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(401);
        result.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var refreshTokenString = "valid_refresh_token";
        var user = new ApplicationUser
        {
            Id = "user_id",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshTokenString))
            .ReturnsAsync(refreshToken);

        _refreshTokenRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("new_access_token");

        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken());

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshTokenString = "expired_token";
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            IsRevoked = false
        };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshTokenString))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(401);
        result.Error.Should().Be("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshTokenString = "nonexistent_token";

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshTokenString))
            .ReturnsAsync((RefreshToken)null);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(401);
        result.Error.Should().Be("Invalid or expired refresh token");
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeAllUserTokens()
    {
        // Arrange
        var userId = "user_id";

        _refreshTokenRepositoryMock.Setup(x => x.RevokeAllUserTokensAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenRepositoryMock.Verify(x => x.RevokeAllUserTokensAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnUser()
    {
        // Arrange
        var userId = "user_id";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be(user.Email);
        result.Value.DisplayName.Should().Be(user.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidUserId_ShouldReturnNotFound()
    {
        // Arrange
        var userId = "nonexistent_user";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
        result.Error.Should().Be("User not found");
    }
}
