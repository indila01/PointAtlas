using PointAtlas.Application.DTOs;

namespace PointAtlas.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request);
    Task<AuthResponseDto> LoginAsync(LoginRequest request);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string userId);
    Task<UserDto> GetCurrentUserAsync(string userId);
}
