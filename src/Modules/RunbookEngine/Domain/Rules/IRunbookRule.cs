namespace RunbookForge.Modules.RunbookEngine.Domain.Rules;

using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Enums;

public interface IRunbookRule
{
    string RuleId { get; }
    string Name { get; }
    string Category { get; }
    LintSeverity DefaultSeverity { get; }

    ValueTask<IReadOnlyList<LintDiagnostic>> EvaluateAsync(RunbookAstDocument ast, CancellationToken ct = default);
}

