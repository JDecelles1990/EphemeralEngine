namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF019_MissingReferencesRule : IRunbookRule
{
    public string RuleId => "RF019";
    public string Name => "Missing References";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasReferencesHeading = ast.RawSectionsFound.Any(s => s.Contains("Reference", StringComparison.OrdinalIgnoreCase));
        var hasLinks = !string.IsNullOrEmpty(ast.RawMarkdown) &&
            (ast.RawMarkdown.Contains("http://", StringComparison.OrdinalIgnoreCase)
             || ast.RawMarkdown.Contains("https://", StringComparison.OrdinalIgnoreCase));

        if (!hasReferencesHeading && !hasLinks)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    "Document lacks external reference links (HTTP/HTTPS) or a '# References' section.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

