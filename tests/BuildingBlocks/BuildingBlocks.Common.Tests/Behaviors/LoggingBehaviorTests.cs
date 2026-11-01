namespace RunbookForge.BuildingBlocks.Common.Tests.Behaviors;

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RunbookForge.BuildingBlocks.Common.Behaviors;
using RunbookForge.BuildingBlocks.Common.Results;
using Xunit;

public class LoggingBehaviorTests
{
    public record SampleCommand(string Data) : IRequest<Result>;

    [Fact]
    public async Task Handle_ShouldCallNextAndLogInformation_WhenExecuted()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<SampleCommand, Result>>>();
        logger.IsEnabled(LogLevel.Information).Returns(true);

        var behavior = new LoggingBehavior<SampleCommand, Result>(logger);
        var command = new SampleCommand("test-data");
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

        logger.ReceivedWithAnyArgs(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}

