namespace RunbookForge.Modules.RunbookEngine.Tests.Workflow;

using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RunbookForge.BuildingBlocks.Common.Results;
using RunbookForge.Modules.RunbookEngine.Application.Features.CompileWorkflow;
using RunbookForge.Modules.RunbookEngine.Domain.Entities;
using Xunit;
using static RunbookForge.Modules.RunbookEngine.Tests.Workflow.WorkflowTestAst;

public sealed class CompileRunbookWorkflowHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public CompileRunbookWorkflowHandlerTests()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private TestRunbookDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestRunbookDbContext>().UseSqlite(_connection).Options);

    private Guid SeedDocument()
    {
        using var context = CreateContext();
        var document = RunbookDocument.Create("Pool Recovery", "pool-recovery", "desc", "# md").Value;
        context.Documents.Add(document);
        context.SaveChanges();
        return document.Id;
    }

    [Fact]
    public async Task Handle_PersistsNewWorkflow_AndReturnsSerializedReactFlowPayload()
    {
        var documentId = SeedDocument();
        await using var context = CreateContext();
        var handler = new CompileRunbookWorkflowHandler(context);

        var result = await handler.Handle(
            new CompileRunbookWorkflowCommand(documentId, Build(LinearSteps(3))),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.Message : string.Empty);
        var dto = result.Value;
        dto.DocumentId.Should().Be(documentId);
        dto.VersionNumber.Should().Be(1);

        using (var nodes = JsonDocument.Parse(dto.NodesJson))
        using (var edges = JsonDocument.Parse(dto.EdgesJson))
        {
            nodes.RootElement.GetArrayLength().Should().Be(5);
            edges.RootElement.GetArrayLength().Should().Be(4);
        }

        await using var verify = CreateContext();
        var stored = await verify.Workflows.SingleAsync();
        stored.Id.Should().Be(dto.WorkflowId);
        stored.DocumentId.Should().Be(documentId);
        stored.NodesJson.Should().Be(dto.NodesJson);
        stored.EdgesJson.Should().Be(dto.EdgesJson);
    }

    [Fact]
    public async Task Handle_UpdatesExistingWorkflow_InsteadOfInsertingDuplicate()
    {
        var documentId = SeedDocument();

        Guid firstWorkflowId;
        await using (var first = CreateContext())
        {
            var created = await new CompileRunbookWorkflowHandler(first).Handle(
                new CompileRunbookWorkflowCommand(documentId, Build(LinearSteps(2))),
                CancellationToken.None);
            firstWorkflowId = created.Value.WorkflowId;
        }

        await using var second = CreateContext();
        var recompiled = await new CompileRunbookWorkflowHandler(second).Handle(
            new CompileRunbookWorkflowCommand(documentId, Build(LinearSteps(4))),
            CancellationToken.None);

        recompiled.IsSuccess.Should().BeTrue(recompiled.IsFailure ? recompiled.Error.Message : string.Empty);
        recompiled.Value.WorkflowId.Should().Be(firstWorkflowId);

        await using var verify = CreateContext();
        var stored = await verify.Workflows.SingleAsync();
        using var nodes = JsonDocument.Parse(stored.NodesJson);
        nodes.RootElement.GetArrayLength().Should().Be(6);
    }

    [Fact]
    public async Task Handle_ReturnsCycleDetectedValidationError_AndPersistsNothing()
    {
        var documentId = SeedDocument();
        await using var context = CreateContext();
        var cyclic = Build([Step("step-1"), Step("step-2", Question("Fixed?", "Yes", "Retry->step-1"))]);

        var result = await new CompileRunbookWorkflowHandler(context).Handle(
            new CompileRunbookWorkflowCommand(documentId, cyclic),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.CycleDetected");
        result.Error.Type.Should().Be(ErrorType.Validation);

        await using var verify = CreateContext();
        (await verify.Workflows.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenDocumentDoesNotExist()
    {
        await using var context = CreateContext();

        var result = await new CompileRunbookWorkflowHandler(context).Handle(
            new CompileRunbookWorkflowCommand(Guid.NewGuid(), Build(LinearSteps(1))),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.DocumentNotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsValidationError_WhenDocumentIdIsEmpty()
    {
        await using var context = CreateContext();

        var result = await new CompileRunbookWorkflowHandler(context).Handle(
            new CompileRunbookWorkflowCommand(Guid.Empty, Build(LinearSteps(1))),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Workflow.InvalidDocumentId");
    }
}

