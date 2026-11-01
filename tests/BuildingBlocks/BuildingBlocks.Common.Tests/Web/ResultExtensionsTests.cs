namespace RunbookForge.BuildingBlocks.Common.Tests.Web;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.BuildingBlocks.Web.Extensions;
using Xunit;

public class ResultExtensionsTests
{
    [Fact]
    public void ToHttpResult_ShouldReturnOk_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var okResult = httpResult.Should().BeOfType<Ok>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void ToHttpResult_ShouldReturnOkWithValue_WhenGenericResultIsSuccess()
    {
        // Arrange
        var result = Result<string>.Success("test payload");

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var okResult = httpResult.Should().BeOfType<Ok<string>>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be("test payload");
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest, "Validation Error")]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden, "Forbidden")]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound, "Resource Not Found")]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict, "State Conflict")]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError, "Server Failure")]
    public void ToHttpResult_ShouldMapToRfc7807ProblemDetails_ForErrorTypes(
        ErrorType errorType,
        int expectedStatusCode,
        string expectedTitle)
    {
        // Arrange
        var errorCode = $"Test.{errorType}";
        var errorMessage = $"Detailed message for {errorType}";
        var error = new Error(errorCode, errorMessage, errorType);
        var result = Result.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problem = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(expectedStatusCode);
        problem.ProblemDetails.Status.Should().Be(expectedStatusCode);
        problem.ProblemDetails.Title.Should().Be(expectedTitle);
        problem.ProblemDetails.Detail.Should().Be(errorMessage);
        problem.ProblemDetails.Extensions.Should().ContainKey("errorCode")
            .WhoseValue.Should().Be(errorCode);
    }

    [Fact]
    public void ToHttpResult_ShouldReturnProblemDetails_WhenGenericResultIsFailure()
    {
        // Arrange
        var error = Error.NotFound("Item.NotFound", "The item does not exist.");
        var result = Result<int>.Failure(error);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problem = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problem.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Title.Should().Be("Resource Not Found");
        problem.ProblemDetails.Detail.Should().Be("The item does not exist.");
        problem.ProblemDetails.Extensions.Should().ContainKey("errorCode")
            .WhoseValue.Should().Be("Item.NotFound");
    }
}

