using PointAtlas.Application.Common;
using PointAtlas.Application.DTOs;

namespace PointAtlas.Application.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<Result> LogoutAsync(string userId);
    Task<Result<UserDto>> GetCurrentUserAsync(string userId);
}
