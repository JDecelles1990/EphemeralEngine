namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed partial class RF025_UnboundedExecutionRule : IRunbookRule
{
    [GeneratedRegex(@"(?:\bping\s+[^\r\n]*-t\b|\btail\s+[^\r\n]*-f\b)", RegexOptions.IgnoreCase)]
    private static partial Regex UnboundedExecutionRegex();

    public string RuleId => "RF025";
    public string Name => "Unbounded Execution";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var diagnostics = new List<LintDiagnostic>();

        foreach (var step in ast.Steps)
        {
            foreach (var command in step.Commands)
            {
                if (string.IsNullOrWhiteSpace(command.Script))
                {
                    continue;
                }

                var matches = UnboundedExecutionRegex().Matches(command.Script);
                foreach (Match match in matches)
                {
                    var cmdText = match.Value;
                    if (cmdText.Contains("tail", StringComparison.OrdinalIgnoreCase) &&
                        (cmdText.Contains("-n") || cmdText.Contains("--lines")))
                    {
                        continue;
                    }

                    diagnostics.Add(new LintDiagnostic(
                        RuleId,
                        DefaultSeverity,
                        $"Step '{step.Id}' contains potentially unbounded polling command '{cmdText.Trim()}'. Specify explicit timeouts or iteration boundaries.",
                        TargetNodeId: step.Id));
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

