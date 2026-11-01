namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed partial class RF028_HardcodedSecretDetectedRule : IRunbookRule
{
    [GeneratedRegex(@"(?:password\s*=\s*['""][^'""]+['""]|AccountKey=[A-Za-z0-9+/=]{40,}|bearer\s+[A-Za-z0-9\-_]{20,}|api[_-]?key\s*=\s*['""][^'""]+['""]|client_secret\s*=\s*['""][^'""]+['""])", RegexOptions.IgnoreCase)]
    private static partial Regex SecretPatternRegex();

    public string RuleId => "RF028";
    public string Name => "Hardcoded Secret Detected";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

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

                var match = SecretPatternRegex().Match(command.Script);
                if (match.Success)
                {
                    diagnostics.Add(new LintDiagnostic(
                        RuleId,
                        DefaultSeverity,
                        $"Step '{step.Id}' contains potential hardcoded secret or credential: '{match.Value}'.",
                        TargetNodeId: step.Id));
                }
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

