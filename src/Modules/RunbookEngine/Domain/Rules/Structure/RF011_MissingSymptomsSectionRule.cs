namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF011_MissingSymptomsSectionRule : IRunbookRule
{
    public string RuleId => "RF011";
    public string Name => "Missing Symptoms Section";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasSymptomsHeading = ast.RawSectionsFound.Any(s => s.Contains("Symptoms", StringComparison.OrdinalIgnoreCase));
        var hasSymptomsItems = ast.Symptoms is { Count: > 0 };

        if (!hasSymptomsHeading || !hasSymptomsItems)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Missing '# Symptoms' section or symptoms list contains 0 items.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
