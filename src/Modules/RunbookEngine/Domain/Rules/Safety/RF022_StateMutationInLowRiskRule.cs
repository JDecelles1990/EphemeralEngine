namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed partial class RF022_StateMutationInLowRiskRule : IRunbookRule
{
    [GeneratedRegex(@"(?:\b(?:Set|Remove|Restart|Stop|Clear|New)-[a-zA-Z0-9]+|\b(?:rm|del|kill)\b|\bformat\b(?!-))", RegexOptions.IgnoreCase)]
    private static partial Regex MutatingCommandRegex();

    public string RuleId => "RF022";
    public string Name => "State Mutation in Low Risk Runbook";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        if (!string.Equals(ast.Metadata.Risk, "low", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (var step in ast.Steps)
        {
            foreach (var command in step.Commands)
            {
                if (string.IsNullOrWhiteSpace(command.Script))
                {
                    continue;
                }

                var match = MutatingCommandRegex().Match(command.Script);
                if (match.Success)
                {
                    diagnostics.Add(new LintDiagnostic(
                        RuleId,
                        DefaultSeverity,
                        $"Low-risk runbook contains mutating command '{match.Value}' in step '{step.Id}'. Mutating operations require at least 'medium' risk classification.",
                        TargetNodeId: step.Id));
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

