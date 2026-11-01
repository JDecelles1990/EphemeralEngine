namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF012_MissingDiagnosticsSectionRule : IRunbookRule
{
    public string RuleId => "RF012";
    public string Name => "Missing Diagnostics Section";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasDiagnosticsHeading = ast.RawSectionsFound.Any(s => s.Contains("Diagnostic", StringComparison.OrdinalIgnoreCase));
        var hasDiagnosticSteps = ast.Steps.Any(s =>
            s.Title.Contains("Diagnos", StringComparison.OrdinalIgnoreCase) ||
            s.Id.Contains("diag", StringComparison.OrdinalIgnoreCase) ||
            s.Commands.Any(c => c.Mode?.Equals("readonly", StringComparison.OrdinalIgnoreCase) == true));

        if (!hasDiagnosticsHeading && !hasDiagnosticSteps)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Missing '# Diagnostics' section and no diagnostic troubleshooting steps were found.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
