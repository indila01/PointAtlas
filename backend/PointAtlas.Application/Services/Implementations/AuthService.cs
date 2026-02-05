using Microsoft.AspNetCore.Identity;
using PointAtlas.Application.Common;
using PointAtlas.Application.DTOs;
using PointAtlas.Application.Services.Interfaces;
using PointAtlas.Core.Entities;
using PointAtlas.Core.Interfaces;

namespace PointAtlas.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<AuthResponseDto>.Conflict("User with this email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<AuthResponseDto>.Failure($"Failed to create user: {errors}");
        }

        // Assign default "User" role
        await _userManager.AddToRoleAsync(user, "User");

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponseDto>.Success(authResponse);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<AuthResponseDto>.Unauthorized("Invalid email or password");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result<AuthResponseDto>.Unauthorized("Invalid email or password");
        }

        var authResponse = await GenerateAuthResponse(user);
        return Result<AuthResponseDto>.Success(authResponse);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (token == null || token.ExpiresAt < DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Unauthorized("Invalid or expired refresh token");
        }

        // Revoke old refresh token
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(token);

        var authResponse = await GenerateAuthResponse(token.User);
        return Result<AuthResponseDto>.Success(authResponse);
    }

    public async Task<Result> LogoutAsync(string userId)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
        return Result.Success();
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<UserDto>.NotFound("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto(user.Id, user.Email ?? string.Empty, user.DisplayName, roles);
        return Result<UserDto>.Success(userDto);
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        var userDto = new UserDto(user.Id, user.Email ?? string.Empty, user.DisplayName, roles);

        return new AuthResponseDto(accessToken, refreshToken, userDto);
    }
}
