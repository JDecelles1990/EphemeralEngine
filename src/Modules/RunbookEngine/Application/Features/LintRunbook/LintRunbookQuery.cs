namespace RunbookForge.Modules.RunbookEngine.Application.Features.LintRunbook;

using RunbookForge.BuildingBlocks.Common.Contracts;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed record LintRunbookQuery(RunbookAstDocument Ast) : IQuery<Result<LinterReport>>;
