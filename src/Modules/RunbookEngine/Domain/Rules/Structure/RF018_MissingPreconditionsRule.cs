namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF018_MissingPreconditionsRule : IRunbookRule
{
    public string RuleId => "RF018";
    public string Name => "Missing Preconditions";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var risk = ast.Metadata.Risk?.Trim() ?? string.Empty;
        var requiresPreconditions = string.Equals(risk, "medium", StringComparison.OrdinalIgnoreCase)
            || string.Equals(risk, "high", StringComparison.OrdinalIgnoreCase)
            || string.Equals(risk, "critical", StringComparison.OrdinalIgnoreCase);

        if (!requiresPreconditions)
        {
            return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
        }

        var hasPreconditionsOrScope = ast.RawSectionsFound.Any(s =>
            s.Contains("Precondition", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Scope", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Prerequisite", StringComparison.OrdinalIgnoreCase));

        if (!hasPreconditionsOrScope)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Runbook with risk level '{risk}' must define a '# Preconditions' or '# Scope' section.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

