namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF016_SectionOrderingDeviationRule : IRunbookRule
{
    public string RuleId => "RF016";
    public string Name => "Section Ordering Deviation";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var diagnostics = new List<LintDiagnostic>();
        var lastCanonicalIndex = -1;
        string? lastCanonicalSection = null;

        foreach (var section in ast.RawSectionsFound)
        {
            var canonicalIndex = GetCanonicalIndex(section);
            if (!canonicalIndex.HasValue)
            {
                continue;
            }

            if (canonicalIndex.Value < lastCanonicalIndex)
            {
                diagnostics.Add(new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Section '{section}' appears out of canonical order after '{lastCanonicalSection}'. Expected order: Symptoms -> Scope -> Diagnostics -> Remediation -> Validation -> Escalation."));
            }
            else
            {
                lastCanonicalIndex = canonicalIndex.Value;
                lastCanonicalSection = section;
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }

    private static int? GetCanonicalIndex(string heading)
    {
        if (heading.Contains("Symptom", StringComparison.OrdinalIgnoreCase)) return 0;
        if (heading.Contains("Scope", StringComparison.OrdinalIgnoreCase)) return 1;
        if (heading.Contains("Diagnos", StringComparison.OrdinalIgnoreCase)) return 2;
        if (heading.Contains("Remediat", StringComparison.OrdinalIgnoreCase)) return 3;
        if (heading.Contains("Validat", StringComparison.OrdinalIgnoreCase) ||
            heading.Contains("Verificat", StringComparison.OrdinalIgnoreCase)) return 4;
        if (heading.Contains("Escalat", StringComparison.OrdinalIgnoreCase)) return 5;
        return null;
    }
}

