namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF020_EmptySectionBodyRule : IRunbookRule
{
    public string RuleId => "RF020";
    public string Name => "Empty Section Body";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        if (ast.EmptySections.Count == 0)
        {
            return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
        }

        var diagnostics = ast.EmptySections.Select(section =>
            new LintDiagnostic(
                RuleId,
                DefaultSeverity,
                $"Section '{section}' has a heading but contains no content, list items, or action steps."))
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

