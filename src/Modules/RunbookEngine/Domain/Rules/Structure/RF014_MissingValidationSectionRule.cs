namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF014_MissingValidationSectionRule : IRunbookRule
{
    public string RuleId => "RF014";
    public string Name => "Missing Validation Section";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasValidation = ast.RawSectionsFound.Any(s =>
            s.Contains("Validation", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Verification", StringComparison.OrdinalIgnoreCase));

        if (!hasValidation)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Missing required '# Validation' section in runbook structure.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

