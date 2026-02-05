using FluentAssertions;
using PointAtlas.Application.Common;
using Xunit;

namespace PointAtlas.Application.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        var expectedValue = "test value";

        // Act
        var result = Result<string>.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(expectedValue);
        result.Error.Should().BeEmpty();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Failure_WithCustomStatusCode_ShouldSetCorrectStatusCode()
    {
        // Arrange
        var errorMessage = "Server error";
        var statusCode = 500;

        // Act
        var result = Result<string>.Failure(errorMessage, statusCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(statusCode);
    }

    [Fact]
    public void NotFound_ShouldCreateResultWith404StatusCode()
    {
        // Arrange
        var errorMessage = "Resource not found";

        // Act
        var result = Result<string>.NotFound(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public void NotFound_WithoutMessage_ShouldUseDefaultMessage()
    {
        // Act
        var result = Result<string>.NotFound();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Resource not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Unauthorized_ShouldCreateResultWith401StatusCode()
    {
        // Arrange
        var errorMessage = "Invalid credentials";

        // Act
        var result = Result<string>.Unauthorized(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Unauthorized_WithoutMessage_ShouldUseDefaultMessage()
    {
        // Act
        var result = Result<string>.Unauthorized();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Unauthorized access");
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Forbidden_ShouldCreateResultWith403StatusCode()
    {
        // Arrange
        var errorMessage = "Access denied";

        // Act
        var result = Result<string>.Forbidden(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public void Forbidden_WithoutMessage_ShouldUseDefaultMessage()
    {
        // Act
        var result = Result<string>.Forbidden();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Forbidden");
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public void Conflict_ShouldCreateResultWith409StatusCode()
    {
        // Arrange
        var errorMessage = "Resource already exists";

        // Act
        var result = Result<string>.Conflict(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(409);
    }
}

public class ResultWithoutValueTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public void NotFound_ShouldCreateResultWith404StatusCode()
    {
        // Arrange
        var errorMessage = "Item not found";

        // Act
        var result = Result.NotFound(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Unauthorized_ShouldCreateResultWith401StatusCode()
    {
        // Arrange
        var errorMessage = "Not authorized";

        // Act
        var result = Result.Unauthorized(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Forbidden_ShouldCreateResultWith403StatusCode()
    {
        // Arrange
        var errorMessage = "Permission denied";

        // Act
        var result = Result.Forbidden(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public void Conflict_ShouldCreateResultWith409StatusCode()
    {
        // Arrange
        var errorMessage = "Duplicate entry";

        // Act
        var result = Result.Conflict(errorMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.StatusCode.Should().Be(409);
    }
}
