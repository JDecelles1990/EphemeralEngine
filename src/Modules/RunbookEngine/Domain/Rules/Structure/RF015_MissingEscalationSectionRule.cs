namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF015_MissingEscalationSectionRule : IRunbookRule
{
    public string RuleId => "RF015";
    public string Name => "Missing Escalation Section";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasEscalationHeading = ast.RawSectionsFound.Any(s => s.Contains("Escalation", StringComparison.OrdinalIgnoreCase));
        var hasEscalationTriggers = ast.EscalationTriggers is { Count: > 0 };

        if (!hasEscalationHeading || !hasEscalationTriggers)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Missing required '# Escalation' section or escalation triggers collection is empty.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

