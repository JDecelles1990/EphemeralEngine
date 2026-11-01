namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed class RF029_MissingRollbackProcedureRule : IRunbookRule
{
    public string RuleId => "RF029";
    public string Name => "Missing Rollback Procedure";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var risk = ast.Metadata.Risk?.Trim() ?? string.Empty;
        var requiresRollback = string.Equals(risk, "high", StringComparison.OrdinalIgnoreCase)
            || string.Equals(risk, "critical", StringComparison.OrdinalIgnoreCase);

        if (!requiresRollback)
        {
            return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
        }

        var hasRollbackSection = ast.RawSectionsFound.Any(s =>
            s.Contains("Rollback", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Backout", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Backup", StringComparison.OrdinalIgnoreCase));

        var hasRollbackStep = ast.Steps.Any(s =>
            s.Id.Contains("rollback", StringComparison.OrdinalIgnoreCase)
            || s.Title.Contains("rollback", StringComparison.OrdinalIgnoreCase)
            || s.Id.Contains("backout", StringComparison.OrdinalIgnoreCase)
            || s.Title.Contains("backout", StringComparison.OrdinalIgnoreCase)
            || s.Title.Contains("backup", StringComparison.OrdinalIgnoreCase));

        if (!hasRollbackSection && !hasRollbackStep)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Runbook with risk level '{risk}' must define a rollback procedure or backup step.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
