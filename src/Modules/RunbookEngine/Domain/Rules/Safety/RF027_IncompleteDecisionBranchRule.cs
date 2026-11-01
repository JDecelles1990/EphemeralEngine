namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed class RF027_IncompleteDecisionBranchRule : IRunbookRule
{
    public string RuleId => "RF027";
    public string Name => "Incomplete Decision Branch";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var diagnostics = new List<LintDiagnostic>();

        foreach (var step in ast.Steps)
        {
            foreach (var question in step.Questions)
            {
                var options = question.Options ?? [];
                var hasEmptyOption = options.Any(string.IsNullOrWhiteSpace);
                var distinctNonEmpty = options
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                if (options.Count < 2 || hasEmptyOption || distinctNonEmpty < 2)
                {
                    diagnostics.Add(new LintDiagnostic(
                        RuleId,
                        DefaultSeverity,
                        $"Decision question '{question.Prompt}' in step '{step.Id}' is incomplete: questions must define at least 2 distinct non-empty branch options.",
                        TargetNodeId: step.Id));
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

