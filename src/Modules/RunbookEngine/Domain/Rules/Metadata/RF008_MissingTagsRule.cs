namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF008_MissingTagsRule : IRunbookRule
{
    public string RuleId => "RF008";
    public string Name => "Missing Tags";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        if (ast.Metadata.Tags is null || ast.Metadata.Tags.Count == 0 || ast.Metadata.Tags.All(string.IsNullOrWhiteSpace))
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "No search tags defined in frontmatter metadata.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
