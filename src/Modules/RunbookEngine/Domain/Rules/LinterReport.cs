namespace RunbookForge.Modules.RunbookEngine.Domain.Rules;

using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed record LinterReport(
    int Score,
    IReadOnlyList<LintDiagnostic> Diagnostics,
    bool HasErrors,
    bool HasWarnings)
{
    public static LinterReport Create(IReadOnlyList<LintDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var errors = 0;
        var warnings = 0;
        var infos = 0;

        foreach (var diagnostic in diagnostics)
        {
            switch (diagnostic.Severity)
            {
                case LintSeverity.Error:
                    errors++;
                    break;
                case LintSeverity.Warning:
                    warnings++;
                    break;
                case LintSeverity.Info:
                    infos++;
                    break;
            }
        }

        var deduction = (errors * 15) + (warnings * 5) + (infos * 1);
        var score = Math.Max(0, 100 - deduction);

        return new LinterReport(
            Score: score,
            Diagnostics: diagnostics,
            HasErrors: errors > 0,
            HasWarnings: warnings > 0);
    }
}

