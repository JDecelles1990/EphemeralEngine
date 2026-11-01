namespace RunbookForge.BuildingBlocks.Common.Tests.Behaviors;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using RunbookForge.BuildingBlocks.Common.Behaviors;
using RunbookForge.BuildingBlocks.Common.Results;
using Xunit;

public class ValidationBehaviorTests
{
    public record TestCommand(string Title) : IRequest<Result>;
    public record TestQuery(int Id) : IRequest<Result<string>>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        }
    }

    [Fact]
    public async Task Handle_ShouldExecuteNext_WhenNoValidatorsRegistered()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);
        var command = new TestCommand("Valid Title");
        var nextExecuted = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextExecuted = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldExecuteNext_WhenValidationPasses()
    {
        // Arrange
        var validators = new IValidator<TestCommand>[] { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);
        var command = new TestCommand("Valid Title");
        var nextExecuted = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextExecuted = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldShortCircuitAndReturnValidationFailure_WhenValidationFails()
    {
        // Arrange
        var validators = new IValidator<TestCommand>[] { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);
        var command = new TestCommand(string.Empty);
        var nextExecuted = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextExecuted = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Request.ValidationFailed");
        result.Error.Message.Should().Contain("Title: Title is required.");
    }

    private sealed class MultiPropertyCommandValidator : AbstractValidator<TestCommand>
    {
        public MultiPropertyCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MinimumLength(5).WithMessage("Title must be at least 5 characters.");
        }
    }

    [Fact]
    public async Task Handle_ShouldAggregateMultipleValidationFailures_WhenMultipleRulesFail()
    {
        // Arrange
        var validator = new InlineValidator<TestCommand>();
        validator.RuleFor(x => x.Title).NotEmpty().WithMessage("Required.");
        validator.RuleFor(x => x.Title).Must(_ => false).WithMessage("Custom rule failed.");

        var behavior = new ValidationBehavior<TestCommand, Result>([validator]);
        var command = new TestCommand(string.Empty);
        var nextExecuted = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextExecuted = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Contain("Title: Required.");
        result.Error.Message.Should().Contain("Title: Custom rule failed.");
    }

    [Fact]
    public async Task Handle_ShouldReturnGenericResultFailure_WhenResponseIsGenericResult()
    {
        // Arrange
        var validator = new InlineValidator<TestQuery>();
        validator.RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be positive.");

        var behavior = new ValidationBehavior<TestQuery, Result<string>>([validator]);
        var query = new TestQuery(-1);
        var nextExecuted = false;

        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextExecuted = true;
            return Task.FromResult(Result<string>.Success("expected-value"));
        };

        // Act
        var result = await behavior.Handle(query, next, CancellationToken.None);

        // Assert
        nextExecuted.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Request.ValidationFailed");
        result.Error.Message.Should().Contain("Id: Id must be positive.");

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }
}
