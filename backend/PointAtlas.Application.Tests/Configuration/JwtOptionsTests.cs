using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using PointAtlas.Infrastructure.Configuration;
using Xunit;

namespace PointAtlas.Application.Tests.Configuration;

public class JwtOptionsTests
{
    [Fact]
    public void JwtOptions_WithValidValues_ShouldPassValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void JwtOptions_WithShortSecret_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "short", // Less than 32 characters
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("32 characters");
    }

    [Fact]
    public void JwtOptions_WithEmptySecret_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = string.Empty,
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void JwtOptions_WithEmptyIssuer_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = string.Empty,
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("Issuer is required");
    }

    [Fact]
    public void JwtOptions_WithEmptyAudience_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = "PointAtlas",
            Audience = string.Empty,
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("Audience is required");
    }

    [Fact]
    public void JwtOptions_WithZeroAccessTokenExpiration_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 0, // Invalid
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("between 1 and 1440");
    }

    [Fact]
    public void JwtOptions_WithExcessiveAccessTokenExpiration_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 2000, // More than 1440 (24 hours)
            RefreshTokenExpirationMinutes = 10080
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("between 1 and 1440");
    }

    [Fact]
    public void JwtOptions_WithZeroRefreshTokenExpiration_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 0 // Invalid
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("between 1 and 43200");
    }

    [Fact]
    public void JwtOptions_WithExcessiveRefreshTokenExpiration_ShouldFailValidation()
    {
        // Arrange
        var options = new JwtOptions
        {
            Secret = "this-is-a-very-long-secret-key-with-more-than-32-characters",
            Issuer = "PointAtlas",
            Audience = "PointAtlasUsers",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 50000 // More than 43200 (30 days)
        };

        // Act
        var validationResults = ValidateModel(options);

        // Assert
        validationResults.Should().ContainSingle();
        validationResults[0].ErrorMessage.Should().Contain("between 1 and 43200");
    }

    [Fact]
    public void JwtOptions_SectionName_ShouldBeJwt()
    {
        // Assert
        JwtOptions.SectionName.Should().Be("Jwt");
    }

    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}
