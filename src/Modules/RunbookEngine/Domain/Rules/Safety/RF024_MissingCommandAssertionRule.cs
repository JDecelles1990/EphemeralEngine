namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Safety;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed class RF024_MissingCommandAssertionRule : IRunbookRule
{
    public string RuleId => "RF024";
    public string Name => "Missing Command Assertion";
    public string Category => "Safety";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var diagnostics = new List<LintDiagnostic>();

        foreach (var step in ast.Steps)
        {
            var isDiagnosticStep = step.Id.Contains("diag", StringComparison.OrdinalIgnoreCase)
                || step.Title.Contains("diag", StringComparison.OrdinalIgnoreCase)
                || step.Title.Contains("check", StringComparison.OrdinalIgnoreCase)
                || step.Title.Contains("inspect", StringComparison.OrdinalIgnoreCase);

            if (isDiagnosticStep && step.Commands.Count > 0 && step.Assertions.Count == 0)
            {
                diagnostics.Add(new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Diagnostic step '{step.Id}' runs commands without an accompanying :::assert verification node.",
                    TargetNodeId: step.Id));
            }
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>(diagnostics);
    }
}

