namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed class RF004_InvalidCategoryRule : IRunbookRule
{
    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "networking", "m365", "identity", "endpoint", "infrastructure", "security"
    };

    public string RuleId => "RF004";
    public string Name => "Invalid Category";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        var category = ast.Metadata.Category?.Trim();
        if (string.IsNullOrWhiteSpace(category) || !ValidCategories.Contains(category))
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Category '{ast.Metadata.Category}' is not recognized. Valid categories are: networking, m365, identity, endpoint, infrastructure, security.")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
