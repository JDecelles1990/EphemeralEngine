namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF010_MissingAuthorOrReviewerRule : IRunbookRule
{
    public string RuleId => "RF010";
    public string Name => "Missing Author or Reviewer";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasAuthor = !string.IsNullOrWhiteSpace(ast.Metadata.Author) &&
                        !ast.Metadata.Author.Equals("Unknown", StringComparison.OrdinalIgnoreCase);
        var hasReviewer = !string.IsNullOrWhiteSpace(ast.Metadata.LastReviewed);

        if (!hasAuthor || !hasReviewer)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Document is missing either author information or last reviewed date.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

