namespace PointAtlas.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    IList<string> Roles
);

public record RefreshTokenRequest(
    string RefreshToken
);
