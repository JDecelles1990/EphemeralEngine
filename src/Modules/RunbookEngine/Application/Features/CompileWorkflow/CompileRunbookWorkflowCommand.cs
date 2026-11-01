namespace RunbookForge.Modules.RunbookEngine.Application.Features.CompileWorkflow;

using RunbookForge.BuildingBlocks.Common.Contracts;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Domain.Ast.Models;

public sealed record CompileRunbookWorkflowCommand(Guid DocumentId, RunbookAstDocument Ast)
    : ICommand<Result<RunbookWorkflowDto>>;

public sealed record RunbookWorkflowDto(
    Guid WorkflowId,
    Guid DocumentId,
    int VersionNumber,
    string NodesJson,
    string EdgesJson,
    DateTimeOffset CompiledAt);

