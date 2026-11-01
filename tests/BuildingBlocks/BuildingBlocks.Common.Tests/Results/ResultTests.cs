namespace RunbookForge.BuildingBlocks.Common.Tests.Results;

using FluentAssertions;
using RunbookForge.BuildingBlocks.Common.Results;
using Xunit;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateValidSuccessResult_WhenCalled()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult_WhenErrorProvided()
    {
        // Arrange
        var error = Error.Failure("Test.Failure", "A test failure occurred.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Code.Should().Be("Test.Failure");
        result.Error.Message.Should().Be("A test failure occurred.");
        result.Error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void GenericResult_Success_ShouldHoldValue_WhenProvided()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void GenericResult_Value_ShouldThrowInvalidOperationException_WhenResultIsFailure()
    {
        // Arrange
        var error = Error.NotFound("Item.NotFound", "The requested item was not found.");
        var result = Result<string>.Failure(error);

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of a failure result cannot be accessed.");
    }

    [Fact]
    public void GenericResult_ImplicitConversion_ShouldReturnSuccess_WhenValueIsNotNull()
    {
        // Act
        Result<string> result = "valid-string";

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("valid-string");
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void GenericResult_ImplicitConversion_ShouldReturnFailure_WhenValueIsNull()
    {
        // Arrange
        string? nullString = null;

        // Act
        Result<string> result = nullString;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
        result.Error.Code.Should().Be("Error.NullValue");
    }

    [Fact]
    public void GenericResult_Failure_ShouldSetIsFailureAndError_WhenProvided()
    {
        // Arrange
        var error = Error.Conflict("Entity.Conflict", "Entity already exists.");

        // Act
        var result = Result<int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    public void Error_FactoryMethods_ShouldAssignCorrectErrorType(ErrorType errorType)
    {
        // Act
        var error = errorType switch
        {
            ErrorType.Failure => Error.Failure("Code.Failure", "Failure"),
            ErrorType.Validation => Error.Validation("Code.Validation", "Validation"),
            ErrorType.NotFound => Error.NotFound("Code.NotFound", "NotFound"),
            ErrorType.Conflict => Error.Conflict("Code.Conflict", "Conflict"),
            ErrorType.Unauthorized => Error.Unauthorized("Code.Unauthorized", "Unauthorized"),
            ErrorType.Forbidden => Error.Forbidden("Code.Forbidden", "Forbidden"),
            _ => throw new ArgumentOutOfRangeException(nameof(errorType))
        };

        // Assert
        error.Type.Should().Be(errorType);
        error.Code.Should().StartWith("Code.");
    }

    private sealed class InvalidResultTestHarness : Result
    {
        public InvalidResultTestHarness(bool isSuccess, Error error) : base(isSuccess, error) { }
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenSuccessWithNonNoneError()
    {
        // Arrange
        var act = () => new InvalidResultTestHarness(true, Error.Failure("Error.Code", "Error"));

        // Act & Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid error state for Result.");
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenFailureWithNoneError()
    {
        // Arrange
        var act = () => new InvalidResultTestHarness(false, Error.None);

        // Act & Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid error state for Result.");
    }
}

