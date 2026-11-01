namespace RunbookForge.BuildingBlocks.Common.Behaviors;

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Processing command/query {RequestName}", requestName);

        var response = await next();

        stopwatch.Stop();
        logger.LogInformation("Processed {RequestName} in {ElapsedMilliseconds}ms",
            requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}

