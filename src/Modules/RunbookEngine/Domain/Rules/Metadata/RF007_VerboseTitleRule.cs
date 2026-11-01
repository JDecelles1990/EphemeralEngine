namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF007_VerboseTitleRule : IRunbookRule
{
    public string RuleId => "RF007";
    public string Name => "Verbose Title";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var title = ast.Metadata.Title?.Trim();
        if (!string.IsNullOrWhiteSpace(title) && title.Length > 80)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Document title exceeds 80 characters ({title.Length} characters). Consider condensing for readability.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
