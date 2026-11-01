namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed partial class RF023_UndeclaredElevationRule : IRunbookRule
{
    [GeneratedRegex(@"(?:Start-Process\s+.*-Verb\s+RunAs|#Requires\s+-RunAsAdministrator|\bsudo\b)", RegexOptions.IgnoreCase)]
    private static partial Regex ElevationPatternRegex();

    public string RuleId => "RF023";
    public string Name => "Undeclared Elevation";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

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

                if (!command.RequiresElevation)
                {
                    var match = ElevationPatternRegex().Match(command.Script);
                    if (match.Success)
                    {
                        diagnostics.Add(new LintDiagnostic(
                            RuleId,
                            DefaultSeverity,
                            $"Step '{step.Id}' contains elevated command execution ('{match.Value}') but does not declare 'elevation=\"true\"' or 'RequiresElevation'.",
                            TargetNodeId: step.Id));
                    }
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

