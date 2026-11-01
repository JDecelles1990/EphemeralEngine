namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF001_MissingFrontmatterRule : IRunbookRule
{
    public string RuleId => "RF001";
    public string Name => "Missing Frontmatter";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var isMissing = !ast.Metadata.HasFrontmatter ||
            (string.IsNullOrWhiteSpace(ast.Metadata.Id) &&
             (string.IsNullOrWhiteSpace(ast.Metadata.Title) || ast.Metadata.Title == "Untitled Runbook") &&
             (string.IsNullOrWhiteSpace(ast.Metadata.Slug) || ast.Metadata.Slug == "untitled-runbook"));

        if (isMissing)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Runbook document is missing required YAML frontmatter metadata.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
