namespace RunbookForge.Modules.RunbookEngine.Application.Features.LintRunbook;

using MediatR;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Rules;

public sealed class LintRunbookHandler(RunbookLinterEngine linterEngine)
    : IRequestHandler<LintRunbookQuery, Result<LinterReport>>
{
    public async Task<Result<LinterReport>> Handle(LintRunbookQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Ast is null)
        {
            return Result<LinterReport>.Failure(
                Error.Validation("LintRunbook.NullAst", "Runbook AST document cannot be null."));
        }

        var report = await linterEngine.LintAsync(request.Ast, cancellationToken);
        return Result<LinterReport>.Success(report);
    }
}

