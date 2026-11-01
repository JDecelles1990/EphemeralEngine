namespace RunbookForge.Modules.RunbookEngine.Domain.Entities;

using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RunbookLintResult
{
    private RunbookLintResult()
    {
    }

    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string RuleId { get; private set; } = string.Empty;
    public LintSeverity Severity { get; private set; } = LintSeverity.Info;
    public int LineNumber { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsValid { get; private set; }
    public int ErrorCount { get; private set; }
    public int WarningCount { get; private set; }
    public string Diagnostics { get; private set; } = "[]";
    public DateTimeOffset EvaluatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public RunbookDocument? Document { get; private set; }

    public static Result<RunbookLintResult> Create(
        Guid documentId,
        int versionNumber,
        string ruleId,
        LintSeverity severity,
        int lineNumber,
        string message,
        bool isValid,
        int errorCount = 0,
        int warningCount = 0,
        string diagnostics = "[]")
    {
        if (documentId == Guid.Empty)
            return Result<RunbookLintResult>.Failure(Error.Validation("LintResult.InvalidDocumentId", "DocumentId cannot be empty."));

        var result = new RunbookLintResult
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = versionNumber,
            RuleId = string.IsNullOrWhiteSpace(ruleId) ? "RF000" : ruleId.Trim(),
            Severity = severity,
            LineNumber = lineNumber < 0 ? 0 : lineNumber,
            Message = message?.Trim() ?? string.Empty,
            IsValid = isValid,
            ErrorCount = errorCount < 0 ? 0 : errorCount,
            WarningCount = warningCount < 0 ? 0 : warningCount,
            Diagnostics = string.IsNullOrWhiteSpace(diagnostics) ? "[]" : diagnostics,
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        return Result<RunbookLintResult>.Success(result);
    }
}

