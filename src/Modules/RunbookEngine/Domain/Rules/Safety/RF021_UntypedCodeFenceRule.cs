namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed partial class RF021_UntypedCodeFenceRule : IRunbookRule
{
    private static readonly HashSet<string> RecognizedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "powershell", "pwsh", "posh", "ps1",
        "bash", "sh", "zsh",
        "cmd", "bat", "batch",
        "python", "py",
        "sql", "tsql", "pgsql",
        "json", "yaml", "yml", "xml",
        "csharp", "cs", "javascript", "js", "typescript", "ts"
    };

    [GeneratedRegex(@"^```\s*$", RegexOptions.Multiline)]
    private static partial Regex EmptyCodeFenceRegex();

    public string RuleId => "RF021";
    public string Name => "Untyped Code Fence";
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
                var lang = command.Language?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lang) || !RecognizedLanguages.Contains(lang))
                {
                    diagnostics.Add(new LintDiagnostic(
                        RuleId,
                        DefaultSeverity,
                        $"Step '{step.Id}' contains a command block with an empty or unrecognized language specifier '{command.Language}'.",
                        TargetNodeId: step.Id));
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(ast.RawMarkdown) && EmptyCodeFenceRegex().IsMatch(ast.RawMarkdown))
        {
            if (diagnostics.Count == 0)
            {
                diagnostics.Add(new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    "Document contains untyped code fence with no language specifier."));
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

