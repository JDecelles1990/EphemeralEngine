namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF009_MissingTargetAudienceRule : IRunbookRule
{
    public string RuleId => "RF009";
    public string Name => "Missing Target Audience";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var hasLevel = !string.IsNullOrWhiteSpace(ast.Metadata.Level);
        var hasAudience = !string.IsNullOrWhiteSpace(ast.Metadata.Audience);

        if (!hasLevel && !hasAudience)
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(RuleId, DefaultSeverity, "Neither support tier level nor target audience is specified.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}

