namespace RunbookForge.Modules.RunbookEngine.Domain.Rules;

using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed record LintDiagnostic(
    string RuleId,
    LintSeverity Severity,
    string Message,
    int? LineNumber = null,
    string? TargetNodeId = null);
