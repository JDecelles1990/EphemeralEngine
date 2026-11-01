namespace RunbookForge.Modules.RunbookEngine.Application.Features.CompileWorkflow;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Application.Contracts;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;
using RunbookForge.Modules.RunbookEngine.Domain.Workflow;

public sealed class CompileRunbookWorkflowHandler(IRunbookDbContext dbContext)
    : IRequestHandler<CompileRunbookWorkflowCommand, Result<RunbookWorkflowDto>>
{
    public async Task<Result<RunbookWorkflowDto>> Handle(
        CompileRunbookWorkflowCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DocumentId == Guid.Empty)
        {
            return Result<RunbookWorkflowDto>.Failure(
                Error.Validation("Workflow.InvalidDocumentId", "DocumentId cannot be empty."));
        }

        if (request.Ast is null)
        {
            return Result<RunbookWorkflowDto>.Failure(
                Error.Validation("Workflow.AstRequired", "A parsed runbook AST is required to compile a workflow."));
        }

        var compilation = WorkflowDagCompiler.Compile(request.Ast);
        if (compilation.IsFailure)
        {
            return Result<RunbookWorkflowDto>.Failure(compilation.Error);
        }

        var graph = compilation.Value;
        var nodesJson = ReactFlowSerializer.SerializeNodes(graph.Nodes);
        var edgesJson = ReactFlowSerializer.SerializeEdges(graph.Edges);

        var document = await dbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (document is null)
        {
            return Result<RunbookWorkflowDto>.Failure(
                Error.NotFound("Workflow.DocumentNotFound", $"Runbook document '{request.DocumentId}' was not found."));
        }

        var workflow = await dbContext.Workflows
            .FirstOrDefaultAsync(w => w.DocumentId == request.DocumentId, cancellationToken);

        if (workflow is null)
        {
            var created = RunbookWorkflow.Create(document.Id, document.CurrentVersionNumber, nodesJson, edgesJson);
            if (created.IsFailure)
            {
                return Result<RunbookWorkflowDto>.Failure(created.Error);
            }

            workflow = created.Value;
            document.SetWorkflow(workflow);
            dbContext.Workflows.Add(workflow);
        }
        else
        {
            workflow.Recompile(document.CurrentVersionNumber, nodesJson, edgesJson);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<RunbookWorkflowDto>.Failure(Error.Conflict(
                "Workflow.ConcurrencyConflict",
                "The runbook was modified concurrently; reload and recompile the workflow."));
        }

        return Result<RunbookWorkflowDto>.Success(new RunbookWorkflowDto(
            workflow.Id,
            workflow.DocumentId,
            workflow.VersionNumber,
            workflow.NodesJson,
            workflow.EdgesJson,
            workflow.CompiledAt));
    }
}

