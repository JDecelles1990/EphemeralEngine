namespace RunbookForge.Modules.RunbookEngine.Domain.Rules;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

public sealed class RunbookLinterEngine
{
    private readonly IReadOnlyList<IRunbookRule> _rules;

    public RunbookLinterEngine(IEnumerable<IRunbookRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.OrderBy(r => r.RuleId, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public IReadOnlyList<IRunbookRule> Rules => _rules;

    public async ValueTask<LinterReport> LintAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var diagnostics = new List<LintDiagnostic>();
        foreach (var rule in _rules)
        {
            ct.ThrowIfCancellationRequested();
            var ruleDiagnostics = await rule.EvaluateAsync(ast, ct);
            if (ruleDiagnostics is { Count: > 0 })
            {
                diagnostics.AddRange(ruleDiagnostics);
            }
        }

        return LinterReport.Create(diagnostics);
    }
}
