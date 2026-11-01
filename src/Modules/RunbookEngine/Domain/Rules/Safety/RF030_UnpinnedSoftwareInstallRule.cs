namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed partial class RF030_UnpinnedSoftwareInstallRule : IRunbookRule
{
    [GeneratedRegex(@"(?:winget\s+install|choco\s+install|Install-Module|Install-Package)\s+([^;\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex PackageInstallRegex();

    [GeneratedRegex(@"(?:--version|-version|-RequiredVersion|-MaximumVersion|-ExactVersion|-v\s+\S+)", RegexOptions.IgnoreCase)]
    private static partial Regex VersionPinRegex();

    public string RuleId => "RF030";
    public string Name => "Unpinned Software Install";
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

                var matches = PackageInstallRegex().Matches(command.Script);
                foreach (Match match in matches)
                {
                    if (!VersionPinRegex().IsMatch(match.Value))
                    {
                        diagnostics.Add(new LintDiagnostic(
                            RuleId,
                            DefaultSeverity,
                            $"Step '{step.Id}' executes unpinned package installation ('{match.Value.Trim()}'). Package installations must specify an explicit version.",
                            TargetNodeId: step.Id));
                    }
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

