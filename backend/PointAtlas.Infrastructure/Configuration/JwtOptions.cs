using System.ComponentModel.DataAnnotations;

namespace PointAtlas.Infrastructure.Configuration;

/// <summary>
/// JWT configuration options with validation.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT Secret is required")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters")]
    public string Secret { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Access token expiration must be between 1 and 1440 minutes")]
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    [Range(1, 43200, ErrorMessage = "Refresh token expiration must be between 1 and 43200 minutes")]
    public int RefreshTokenExpirationMinutes { get; set; } = 10080; // 7 days
}
