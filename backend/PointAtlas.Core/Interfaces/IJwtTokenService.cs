using PointAtlas.Core.Entities;

namespace PointAtlas.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    Task<bool> ValidateTokenAsync(string token);
}
