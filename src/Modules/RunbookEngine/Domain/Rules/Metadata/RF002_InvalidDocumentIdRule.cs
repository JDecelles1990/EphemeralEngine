namespace RunbookForge.Modules.RunbookEngine.Domain.Rules.Metadata;

using System.Text.RegularExpressions;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public sealed partial class RF002_InvalidDocumentIdRule : IRunbookRule
{
    [GeneratedRegex(@"^[A-Z]{2,6}-[0-9]{3,5}$")]
    private static partial Regex DocumentIdRegex();

    public string RuleId => "RF002";
    public string Name => "Invalid Document ID";
    public string Category => "Metadata";
    public LintSeverity DefaultSeverity => LintSeverity.Error;

    public ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ast);

        if (string.IsNullOrWhiteSpace(ast.Metadata.Id) || !DocumentIdRegex().IsMatch(ast.Metadata.Id.Trim()))
        {
            IReadOnlyList<LintDiagnostic> diagnostics =
            [
                new LintDiagnostic(
                    RuleId,
                    DefaultSeverity,
                    $"Document ID '{ast.Metadata.Id}' is invalid. Expected format matching '^[A-Z]{{2,6}}-[0-9]{{3,5}}$' (e.g. NET-101, SEC-1001).")
            ];
            return ValueTask.FromResult(diagnostics);
        }

        return ValueTask.FromResult<IReadOnlyList<LintDiagnostic>>([]);
    }
}
