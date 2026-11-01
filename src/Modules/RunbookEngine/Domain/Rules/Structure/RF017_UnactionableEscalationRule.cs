namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Structure;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed partial class RF017_UnactionableEscalationRule : IRunbookRule
{
    [GeneratedRegex(@"(?:team|tier\s*[123]|t[123]|on-?call|sre|noc|devops|infra|engineering|support|helpdesk|slack|teams|pagerduty|jira|servicenow|ticket|queue|@|\bcontact\b|\bcall\b)", RegexOptions.IgnoreCase)]
    private static partial Regex ActionableTargetRegex();

    public string RuleId => "RF017";
    public string Name => "Unactionable Escalation";
    public string Category => "Structure";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasEscalationHeading = ast.RawSectionsFound.Any(s => s.Contains("Escalation", StringComparison.OrdinalIgnoreCase));
        if (!hasEscalationHeading && ast.EscalationTriggers.Count == 0)
        {
            // Missing section handled by RF015
            return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
        }

        var isActionable = ast.EscalationTriggers.Any(trigger => ActionableTargetRegex().IsMatch(trigger));

        if (!isActionable && !string.IsNullOrWhiteSpace(ast.RawMarkdown))
        {
            // Also inspect markdown content under Escalation if triggers list was empty or unstructured
            var escalationIndex = ast.RawMarkdown.IndexOf("# Escalation", StringComparison.OrdinalIgnoreCase);
            if (escalationIndex >= 0)
            {
                var contentAfterEscalation = ast.RawMarkdown[escalationIndex..];
                isActionable = ActionableTargetRegex().IsMatch(contentAfterEscalation);
            }
        }

        if (!isActionable)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    "The '# Escalation' section contains no explicit escalation contact, team, or ticket queue reference.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

