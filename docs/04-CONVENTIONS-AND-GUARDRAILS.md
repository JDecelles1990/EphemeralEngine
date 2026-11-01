# RunbookForge — Architectural Conventions & Guardrails
**Document ID:** `docs/04-CONVENTIONS-AND-GUARDRAILS.md`  
**Version:** 1.1.0  
**Status:** FROZEN  
**System:** RunbookForge (Workspace: `EphemeralEngine`)  

> **Context Dependencies:**  
> - **Source of Truth:** Global Invariants & Guardrails (`04-CONVENTIONS-AND-GUARDRAILS.md`)
> - **System Architecture:** [00-ARCHITECTURE.md](./00-ARCHITECTURE.md)  
> - **Implementation SOP:** [03-AI-FEATURE-SOP.md](./03-AI-FEATURE-SOP.md)  
> - **C# Standards:** [standards/CSHARP-STANDARDS.md](./standards/CSHARP-STANDARDS.md)  
> - **TypeScript Standards:** [standards/TYPESCRIPT-REACT-STANDARDS.md](./standards/TYPESCRIPT-REACT-STANDARDS.md)  
> - **F7Hub Security Barrier:** [database/DB-06-EXTERNAL-F7HUB-READONLY.md](./database/DB-06-EXTERNAL-F7HUB-READONLY.md)

---

## 1. Global Compiler & Runtime Invariants

All projects in `EphemeralEngine` must adhere to these compile-time settings configured in `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13.0</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
  </PropertyGroup>
</Project>
```

---

## 2. Definitive Result Pattern Implementation

RunbookForge prohibits throwing business domain exceptions for flow control. All operations communicate status via `Result<T>` or `Result`.

```csharp
namespace RunbookForge.BuildingBlocks.Common.Results;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}

public readonly record struct Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.", ErrorType.Failure);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Invalid error state for Result.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);
    public static new Result<TValue> Failure(Error error) => new(default, false, error);
    
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure(Error.NullValue);
}
```

---

## 3. HTTP ProblemDetails Mapping Matrix

Minimal APIs transform `Result` failures into RFC 7807 `ProblemDetails` via an extension method in `BuildingBlocks.Web`:

| `ErrorType` | HTTP Status Code | RFC Title | Usage Scenario |
| :--- | :--- | :--- | :--- |
| `Validation` | **400 Bad Request** | Bad Request | FluentValidation rule failures, syntax errors. |
| `Unauthorized` | **401 Unauthorized** | Unauthorized | Missing or expired authentication token. |
| `Forbidden` | **403 Forbidden** | Forbidden | Insufficient permissions for tenant or resource. |
| `NotFound` | **404 Not Found** | Not Found | Requested entity ID does not exist in module schema. |
| `Conflict` | **409 Conflict** | Conflict | Unique slug collision, concurrency token mismatch (`xmin`). |
| `Failure` | **500 Internal Error** | Internal Server Error | Unrecoverable persistence or infrastructure failure. |

```csharp
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
                detail: result.Error.Message)
        };
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess) return Results.Ok(result.Value);
        return ((Result)result).ToHttpResult();
    }
}
```

---

## 4. SQLite Read-Only Security Guardrails

Under no circumstances shall any code modify the legacy F7Hub SQLite database.

1. **Access Scope:** Access is restricted exclusively to `RunbookForge.Modules.TicketIngestion.Infrastructure`.
2. **Immutable Connection String:** Must enforce `Data Source=f7hub.db;Mode=ReadOnly;Cache=Shared;`.
3. **No-Tracking:** `QueryTrackingBehavior.NoTracking` is globally active.
4. **Hard Exception on Mutation:** Any invocation of `SaveChangesAsync()` or `SaveChanges()` unconditionally throws `InvalidOperationException`.

For the complete `F7HubReadOnlyDbContext` class declaration, legacy table mappings, and service registration, see the canonical [F7Hub Read-Only Adapter Specification](./database/DB-06-EXTERNAL-F7HUB-READONLY.md#2-hardened-f7hubreadonlydbcontext-implementation).

---

## 5. Architectural Anti-Patterns Catalog

Agents and developers MUST avoid these forbidden patterns:

| Prohibited Anti-Pattern | Reason for Prohibition | Compliant Alternative |
| :--- | :--- | :--- |
| **Cross-Module `DbContext` Injection** | Creates runtime database coupling and breaks module autonomy. | Send a MediatR query or command across modules. |
| **Direct Cross-Schema Foreign Keys** | Prevents independent schema migrations and data sharding. | Store foreign IDs as plain `Guid` fields without SQL FK constraints. |
| **Throwing Exceptions for Business Errors** | Degrades performance and violates deterministic API contracts. | Return `Result.Failure(Error.Validation(...))`. |
| **Using `any` in TypeScript Frontend** | Destroys end-to-end type safety between C# DTOs and React UI. | Define explicit TypeScript interfaces matching backend DTOs. |
| **Raw SQL String Queries without Parameters** | Introduces SQL injection risks and bypasses EF Core type safety. | Use EF Core LINQ projections or parameterized Dapper queries. |
| **Writing to F7Hub SQLite** | Corrupts legacy incident archives. | Read via `F7HubReadOnlyDbContext` with `Mode=ReadOnly;`. |
