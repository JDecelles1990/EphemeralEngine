namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF003_MissingTitleRule : IRunbookRule
{
    public string RuleId => "RF003";
    public string Name => "Missing Title";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var title = ast.Metadata.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title) || title.Length < 5)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Document title is missing or too short (minimum 5 characters required).")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

