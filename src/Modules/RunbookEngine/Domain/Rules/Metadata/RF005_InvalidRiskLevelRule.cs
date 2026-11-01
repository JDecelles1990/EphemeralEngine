namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF005_InvalidRiskLevelRule : IRunbookRule
{
    private static readonly HashSet<string> ValidRisks = new(StringComparer.OrdinalIgnoreCase)
    {
        "low", "medium", "high", "critical"
    };

    public string RuleId => "RF005";
    public string Name => "Invalid Risk Level";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var risk = ast.Metadata.Risk?.Trim();
        if (string.IsNullOrWhiteSpace(risk) || !ValidRisks.Contains(risk))
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Risk level '{ast.Metadata.Risk}' is invalid. Allowed risk levels are: low, medium, high, critical.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

