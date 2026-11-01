namespace RunbookForge.BuildingBlocks.Web.Extensions;

using Microsoft.AspNetCore.Http;
using RunbookForge.BuildingBlocks.Common.Results;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess) return Results.Ok();

        return result.Error.Type switch
        {
            ErrorType.Validation => Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?> { ["errorCode"] = result.Error.Code }),

            ErrorType.Unauthorized => Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?> { ["errorCode"] = result.Error.Code }),

            ErrorType.Forbidden => Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?> { ["errorCode"] = result.Error.Code }),

            ErrorType.NotFound => Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found",
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?> { ["errorCode"] = result.Error.Code }),

            ErrorType.Conflict => Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "State Conflict",
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?> { ["errorCode"] = result.Error.Code }),

            _ => Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Failure",
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?> { ["errorCode"] = result.Error.Code })
        };
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess) return Results.Ok(result.Value);
        return ((Result)result).ToHttpResult();
    }
}

