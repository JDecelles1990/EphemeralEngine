namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed class RF026_MissingRemediationValidationRule : IRunbookRule
{
    public string RuleId => "RF026";
    public string Name => "Missing Remediation Validation";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasValidationSection = ast.RawSectionsFound.Any(s =>
            s.Contains("Validation", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Verification", StringComparison.OrdinalIgnoreCase));

        var hasValidationStep = ast.Steps.Any(s =>
            s.Id.Contains("val", StringComparison.OrdinalIgnoreCase)
            || s.Title.Contains("validat", StringComparison.OrdinalIgnoreCase)
            || s.Title.Contains("verif", StringComparison.OrdinalIgnoreCase));

        var remediationSteps = ast.Steps.Where(s =>
            (s.Id.Contains("rem", StringComparison.OrdinalIgnoreCase)
             || s.Title.Contains("remediat", StringComparison.OrdinalIgnoreCase)
             || s.Title.Contains("fix", StringComparison.OrdinalIgnoreCase))
            && s.Commands.Count > 0)
            .ToList();

        if (remediationSteps.Count == 0)
        {
            return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
        }

        var anyRemediationHasAssertions = remediationSteps.Any(s => s.Assertions.Count > 0);

        if (!hasValidationSection && !hasValidationStep && !anyRemediationHasAssertions)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    "Remediation commands execute without subsequent verification instructions, assertions, or validation steps.",
                    TargetNodeId: remediationSteps[0].Id)
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

